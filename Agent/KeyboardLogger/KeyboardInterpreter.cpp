// ====================================================== { START OF FILE } ====================================================== //
#include "KeyboardInterpreter.h"

#include <Windows.h>

#include <array>
#include <string>

IntentEvent KeyboardInterpreter::interpret(
    const KeyboardEvent &event,
    const KeyboardState &state) const
{
    const HKL keyboardLayout =
        getActiveKeyboardLayout();

    return interpretKey(
        event,
        state,
        keyboardLayout);
}

HKL KeyboardInterpreter::getActiveKeyboardLayout() const
{
    const HWND foreground =
        GetForegroundWindow();

    if (foreground == nullptr)
    {
        return GetKeyboardLayout(0);
    }

    const DWORD threadId =
        GetWindowThreadProcessId(
            foreground,
            nullptr);

    if (threadId == 0)
    {
        return GetKeyboardLayout(0);
    }

    const HKL layout =
        GetKeyboardLayout(threadId);

    if (layout == nullptr)
    {
        return GetKeyboardLayout(0);
    }

    return layout;
}

std::wstring KeyboardInterpreter::translateToText(
    const KeyboardEvent &event,
    const KeyboardState &state,
    HKL keyboardLayout) const
{
    BYTE keyboardState[256]{};

    if (!GetKeyboardState(keyboardState))
    {
        return {};
    }

    // Use our own tracked modifier state rather than relying entirely
    // on the asynchronous Windows keyboard state.

    keyboardState[VK_LSHIFT] =
        state.shiftDown() ? 0x80 : 0;

    keyboardState[VK_RSHIFT] =
        state.shiftDown() ? 0x80 : 0;

    keyboardState[VK_LCONTROL] =
        state.ctrlDown() ? 0x80 : 0;

    keyboardState[VK_RCONTROL] =
        state.ctrlDown() ? 0x80 : 0;

    keyboardState[VK_LMENU] =
        state.altDown() ? 0x80 : 0;

    keyboardState[VK_RMENU] =
        state.altDown() ? 0x80 : 0;

    keyboardState[VK_LWIN] =
        state.winDown() ? 0x80 : 0;

    keyboardState[VK_RWIN] =
        state.winDown() ? 0x80 : 0;

    wchar_t buffer[32]{};

    const int result =
        ToUnicodeEx(
            event.virtualKey,
            event.scanCode,
            keyboardState,
            buffer,
            static_cast<int>(std::size(buffer)),
            0,
            keyboardLayout);

    if (result > 0)
    {
        return std::wstring(
            buffer,
            buffer + result);
    }

    // A negative result means a dead key.
    //
    // We intentionally don't emit the dead key as completed text.
    if (result < 0)
    {
        return {};
    }

    return {};
}

std::wstring KeyboardInterpreter::getKeyName(
    const KeyboardEvent &event,
    HKL keyboardLayout) const
{
    switch (event.virtualKey)
    {
    case VK_LSHIFT:
        return L"LeftShift";

    case VK_RSHIFT:
        return L"RightShift";

    case VK_LCONTROL:
        return L"LeftCtrl";

    case VK_RCONTROL:
        return L"RightCtrl";

    case VK_LMENU:
        return L"LeftAlt";

    case VK_RMENU:
        return L"RightAlt";

    case VK_LWIN:
        return L"LeftWin";

    case VK_RWIN:
        return L"RightWin";
    }

    switch (event.virtualKey)
    {
    case VK_RETURN:
        return L"Enter";

    case VK_ESCAPE:
        return L"Escape";

    case VK_TAB:
        return L"Tab";

    case VK_BACK:
        return L"Backspace";

    case VK_DELETE:
        return L"Delete";

    case VK_INSERT:
        return L"Insert";

    case VK_HOME:
        return L"Home";

    case VK_END:
        return L"End";

    case VK_PRIOR:
        return L"PageUp";

    case VK_NEXT:
        return L"PageDown";

    case VK_LEFT:
        return L"Left";

    case VK_RIGHT:
        return L"Right";

    case VK_UP:
        return L"Up";

    case VK_DOWN:
        return L"Down";

    case VK_SPACE:
        return L"Space";

    case VK_PAUSE:
        return L"Pause";

    case VK_CAPITAL:
        return L"CapsLock";

    case VK_NUMLOCK:
        return L"NumLock";

    case VK_SCROLL:
        return L"ScrollLock";

    case VK_SNAPSHOT:
        return L"PrintScreen";
    }

    if (event.virtualKey >= VK_F1 &&
        event.virtualKey <= VK_F24)
    {
        return L"F" +
               std::to_wstring(
                   event.virtualKey - VK_F1 + 1);
    }

    switch (event.virtualKey)
    {
    case VK_NUMPAD0:
        return L"Numpad0";

    case VK_NUMPAD1:
        return L"Numpad1";

    case VK_NUMPAD2:
        return L"Numpad2";

    case VK_NUMPAD3:
        return L"Numpad3";

    case VK_NUMPAD4:
        return L"Numpad4";

    case VK_NUMPAD5:
        return L"Numpad5";

    case VK_NUMPAD6:
        return L"Numpad6";

    case VK_NUMPAD7:
        return L"Numpad7";

    case VK_NUMPAD8:
        return L"Numpad8";

    case VK_NUMPAD9:
        return L"Numpad9";

    case VK_DECIMAL:
        return L"NumpadDecimal";

    case VK_ADD:
        return L"NumpadAdd";

    case VK_SUBTRACT:
        return L"NumpadSubtract";

    case VK_MULTIPLY:
        return L"NumpadMultiply";

    case VK_DIVIDE:
        return L"NumpadDivide";
    }

    UINT scanCodeWithFlags =
        event.scanCode << 16;

    if (event.flags & LLKHF_EXTENDED)
    {
        scanCodeWithFlags |= (1u << 24);
    }

    wchar_t name[128]{};

    const int length =
        GetKeyNameTextW(
            static_cast<LONG>(scanCodeWithFlags),
            name,
            static_cast<int>(std::size(name)));

    if (length > 0)
    {
        return std::wstring(
            name,
            name + length);
    }

    return L"VK_" +
           std::to_wstring(event.virtualKey);
}

std::wstring KeyboardInterpreter::getModifierPrefix(
    const KeyboardState &state) const
{
    std::wstring result;

    if (state.ctrlDown())
    {
        result += L"Ctrl+";
    }

    if (state.altDown())
    {
        result += L"Alt+";
    }

    if (state.shiftDown())
    {
        result += L"Shift+";
    }

    if (state.winDown())
    {
        result += L"Win+";
    }

    return result;
}

bool KeyboardInterpreter::capsLockOn() const noexcept
{
    return (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
}

bool KeyboardInterpreter::numLockOn() const noexcept
{
    return (GetKeyState(VK_NUMLOCK) & 0x0001) != 0;
}

bool KeyboardInterpreter::scrollLockOn() const noexcept
{
    return (GetKeyState(VK_SCROLL) & 0x0001) != 0;
}

IntentEvent KeyboardInterpreter::interpretKey(
    const KeyboardEvent &event,
    const KeyboardState &state,
    HKL keyboardLayout) const
{
    IntentEvent result{};

    result.timestamp =
        std::chrono::steady_clock::now();

    result.focusedWindow = event.foregroundWindow;
    result.focusedProcessId = event.foregroundProcessId;
    result.focusedWindowTitle = event.foregroundWindowTitle;

    const bool isModifier =
        event.virtualKey == VK_LSHIFT ||
        event.virtualKey == VK_RSHIFT ||
        event.virtualKey == VK_LCONTROL ||
        event.virtualKey == VK_RCONTROL ||
        event.virtualKey == VK_LMENU ||
        event.virtualKey == VK_RMENU ||
        event.virtualKey == VK_LWIN ||
        event.virtualKey == VK_RWIN;

    if (isModifier)
    {
        result.type =
            IntentType::Modifier;

        result.meaning =
            getKeyName(
                event,
                keyboardLayout);

        return result;
    }

    const std::wstring text =
        translateToText(
            event,
            state,
            keyboardLayout);

    const bool ctrl =
        state.ctrlDown();

    const bool alt =
        state.altDown();

    const bool win =
        state.winDown();

    // Ordinary text takes priority over Shift.
    if (!text.empty() &&
        !ctrl &&
        !alt &&
        !win)
    {
        result.type =
            IntentType::Text;

        result.text = text;
        result.meaning = text;

        return result;
    }

    if (ctrl ||
        alt ||
        win)
    {
        result.type =
            IntentType::Shortcut;

        result.meaning =
            getModifierPrefix(state) +
            getKeyName(
                event,
                keyboardLayout);

        return result;
    }

    result.type =
        IntentType::SpecialKey;

    result.meaning =
        getKeyName(
            event,
            keyboardLayout);

    return result;
}
// ====================================================== { END OF FILE } ====================================================== //