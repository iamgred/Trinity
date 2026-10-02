#include "KeyboardLogger.h"
#include <iostream>

KeyboardLogger *KeyboardLogger::instance = nullptr;

// =============================================== Start Logging ===============================================
bool KeyboardLogger::Start()
{
    if (keyboardHook != nullptr)
        return false; // Already started

    instance = this; // Set the singleton instance

    startTime = std::chrono::steady_clock::now();

    keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardProc, GetModuleHandle(nullptr), 0);

    if (keyboardHook == nullptr)
    {
        std::cerr
            << "Failed to set keyboard hook. Error: "
            << GetLastError()
            << '\n';
        instance = nullptr; // Reset the singleton instance on failure
        return false;
    }

    return true;
}

// =============================================== Stop Logging ===============================================
void KeyboardLogger::Stop()
{
    if (keyboardHook != nullptr)
    {
        UnhookWindowsHookEx(keyboardHook);
        keyboardHook = nullptr;
    }
    instance = nullptr; // Reset the singleton instance
}

// =============================================== Get Key Events ===============================================
const std::vector<KeyEvent> &KeyboardLogger::GetKeyEvents() const
{
    return keyEvents;
}

// =============================================== Modifier States ===============================================
void KeyboardLogger::UpdateModifierStates(DWORD vk, bool down)
{
    switch (vk)
    {
    case VK_LSHIFT:
        shiftlDown = down;
        break;
    case VK_RSHIFT:
        shiftrDown = down;
        break;
    case VK_LCONTROL:
        ctrllDown = down;
        break;
    case VK_RCONTROL:
        ctrlrDown = down;
        break;
    case VK_LMENU:
        altlDown = down;
        break;
    case VK_RMENU:
        altrDown = down;
        break;
    case VK_LWIN:
    case VK_RWIN:
        winDown = down;
        break;
    default:
        break;
    }
}

//=============================================== Keyboard Hook Callback ===============================================
LRESULT CALLBACK KeyboardLogger::KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode != HC_ACTION || instance == nullptr)
    {
        return CallNextHookEx(nullptr, nCode, wParam, lParam);
    }

    const auto *keyboard = reinterpret_cast<KBDLLHOOKSTRUCT *>(lParam);

    // Determine if the key event is a key down or key up event
    bool keyDown = (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN);
    bool keyUp = (wParam == WM_KEYUP || wParam == WM_SYSKEYUP);

    if (keyDown || keyUp)
    {
        // Update the modifier states
        instance->UpdateModifierStates(keyboard->vkCode, keyDown);

        // Create a KeyEvent object and populate its fields
        KeyEvent event;
        // Timing information
        event.timestamp = std::chrono::steady_clock::now();
        event.elapsedMilliseconds = static_cast<std::uint64_t>(std::chrono::duration_cast<std::chrono::milliseconds>(event.timestamp - instance->startTime).count());
        // Raw keyboard information
        event.vkCode = keyboard->vkCode;
        event.scanCode = keyboard->scanCode;
        event.flags = keyboard->flags;
        // Key event states
        event.keyDown = keyDown;
        event.keyUp = keyUp;
        // Modifier key states
        event.shiftl = instance->shiftlDown;
        event.shiftr = instance->shiftrDown;
        event.ctrll = instance->ctrllDown;
        event.ctrlr = instance->ctrlrDown;
        event.altl = instance->altlDown;
        event.altr = instance->altrDown;
        event.win = instance->winDown;
        // AltGr state is determined by the right Alt key and the Ctrl key
        event.altgr = instance->altrDown && instance->ctrlrDown;
        // Lock key states
        event.capsLock = (GetKeyState(VK_CAPITAL) & 0x0001) != 0;
        event.numLock = (GetKeyState(VK_NUMLOCK) & 0x0001) != 0;

        // Store the KeyEvent in the vector
        instance->keyEvents.push_back(event);

        // TODO: Temp diagnostic output to console for testing
        std::cout << (event.keyDown ? "Key Down: " : "Key Up: ")
                  << "VK: " << event.vkCode
                  << ", Scan: " << event.scanCode
                  << "\n";
    }
    // Call the next hook in the chain
    return CallNextHookEx(
        instance != nullptr ? instance->keyboardHook : nullptr,
        nCode,
        wParam,
        lParam);
}
