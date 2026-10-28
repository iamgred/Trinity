#pragma once

#include <windows.h>

#include <chrono>
#include <cstdint>
#include <string>
#include <vector>

struct KeyEvent
{
    // Timestamp of the key event
    std::chrono::steady_clock::time_point timestamp;

    // Elapsed time in milliseconds since the start of logging
    std::uint64_t elapsedMilliseconds;

    // Virtual key code and scan code of the key event
    DWORD vkCode;
    DWORD scanCode;
    DWORD flags;

    // State of the key event (down or up)
    bool keyDown;
    bool keyUp;
    // State of modifier keys
    bool shiftl;
    bool shiftr;
    bool ctrll;
    bool ctrlr;
    bool altl;
    bool altr;
    bool altgr;
    // State of lock keys
    bool capsLock;
    bool numLock;
    // State of the Windows key
    bool win;
};

// KeyboardLogger class for logging keyboard events
class KeyboardLogger
{
public:
    bool Start();
    void Stop();

    const std::vector<KeyEvent> &GetKeyEvents() const;

private:
    static LRESULT CALLBACK KeyboardProc(int nCode, WPARAM wParam, LPARAM lParam);
    void UpdateModifierStates(DWORD vk, bool down);

private:
    HHOOK keyboardHook = nullptr;

    bool shiftlDown = false;
    bool shiftrDown = false;
    bool ctrllDown = false;
    bool ctrlrDown = false;
    bool altlDown = false;
    bool altrDown = false;
    bool winDown = false;

    std::chrono::steady_clock::time_point startTime;
    std::vector<KeyEvent> keyEvents;

    static KeyboardLogger *instance;
};