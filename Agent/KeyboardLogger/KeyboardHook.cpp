// ====================================================== { START OF FILE } ====================================================== //
#include "KeyboardHook.h"

#include <Windows.h>

KeyboardHook *KeyboardHook::activeHook_ = nullptr;

KeyboardHook::KeyboardHook(
    Callback callback)
    : callback_(std::move(callback))
{
}

KeyboardHook::~KeyboardHook()
{
    stop();
}

bool KeyboardHook::start()
{
    if (hook_ != nullptr)
    {
        return true;
    }

    if (activeHook_ != nullptr)
    {
        return false;
    }

    HINSTANCE module =
        GetModuleHandleW(nullptr);

    hook_ =
        SetWindowsHookExW(
            WH_KEYBOARD_LL,
            &KeyboardHook::hookProc,
            module,
            0);

    if (hook_ == nullptr)
    {
        return false;
    }

    activeHook_ = this;

    return true;
}

void KeyboardHook::stop()
{
    if (hook_ == nullptr)
    {
        return;
    }

    UnhookWindowsHookEx(hook_);

    hook_ = nullptr;

    if (activeHook_ == this)
    {
        activeHook_ = nullptr;
    }
}

bool KeyboardHook::isRunning() const noexcept
{
    return hook_ != nullptr;
}

LRESULT CALLBACK KeyboardHook::hookProc(
    int nCode,
    WPARAM wParam,
    LPARAM lParam)
{
    if (nCode != HC_ACTION ||
        activeHook_ == nullptr)
    {
        return CallNextHookEx(
            nullptr,
            nCode,
            wParam,
            lParam);
    }

    const auto *keyboard =
        reinterpret_cast<
            const KBDLLHOOKSTRUCT *>(lParam);

    if (keyboard == nullptr)
    {
        return CallNextHookEx(
            activeHook_->hook_,
            nCode,
            wParam,
            lParam);
    }

    const bool keyDown =
        wParam == WM_KEYDOWN ||
        wParam == WM_SYSKEYDOWN;

    const bool keyUp =
        wParam == WM_KEYUP ||
        wParam == WM_SYSKEYUP;

    if (!keyDown && !keyUp)
    {
        return CallNextHookEx(
            activeHook_->hook_,
            nCode,
            wParam,
            lParam);
    }

    KeyboardEvent event{};

    event.virtualKey =
        keyboard->vkCode;

    event.scanCode =
        keyboard->scanCode;

    event.flags =
        keyboard->flags;

    // Window focus
    HWND foregroundWindow = GetForegroundWindow();

    event.foregroundWindow = foregroundWindow;

    if (foregroundWindow != nullptr)
    {
        DWORD processId = 0;
        GetWindowThreadProcessId(foregroundWindow, &processId);
        event.foregroundProcessId = processId;

        wchar_t windowTitle[256] = {};

        GetWindowTextW(
            foregroundWindow,
            windowTitle,
            256);

        event.foregroundWindowTitle = windowTitle;
    }

    activeHook_->handleEvent(event);

    return CallNextHookEx(
        activeHook_->hook_,
        nCode,
        wParam,
        lParam);
}

void KeyboardHook::handleEvent(
    const KeyboardEvent &event)
{
    if (callback_)
    {
        callback_(event);
    }
}

// ====================================================== { END OF FILE } ====================================================== //