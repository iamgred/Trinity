// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include <Windows.h>
#include <string>

// Represents a single low-level keyboard event.
struct KeyboardEvent
{
    // Windows virtual-key code.
    DWORD virtualKey{};

    // Hardware scan code.
    DWORD scanCode{};

    // Flags supplied by the Windows keyboard hook.
    DWORD flags{};

    // The handle of the foreground window.
    HWND foregroundWindow{};
    // The process ID of the foreground window.
    DWORD foregroundProcessId{};
    // The title of the foreground window.
    std::wstring foregroundWindowTitle{};

    // Returns true when this event represents a key-down action.
    [[nodiscard]]
    bool isKeyDown() const noexcept
    {
        return (flags & LLKHF_UP) == 0;
    }

    // Returns true when this event represents a key-up action.
    [[nodiscard]]
    bool isKeyUp() const noexcept
    {
        return !isKeyDown();
    }

    // Returns true when Windows reports that the event was injected.
    [[nodiscard]]
    bool isInjected() const noexcept
    {
        return (flags & LLKHF_INJECTED) != 0;
    }
};

// ====================================================== { END OF FILE } ====================================================== //