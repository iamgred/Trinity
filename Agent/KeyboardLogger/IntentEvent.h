// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include <Windows.h>

#include <chrono>
#include <string>

enum class IntentType
{
    Text,
    Shortcut,
    SpecialKey,
    Modifier
};

struct IntentEvent
{
    // Timestamp of the intent event
    using Timestamp = std::chrono::steady_clock::time_point;
    Timestamp timestamp;
    // Type of the intent event
    IntentType type{IntentType::SpecialKey};

    // Meaning of the intent event
    std::wstring meaning;

    // Text associated with the intent event
    std::wstring text;

    // Window that is focused on
    HWND focusedWindow{nullptr};

    // Process ID of the focused window
    DWORD focusedProcessId{0};

    // Title of the focused window
    std::wstring focusedWindowTitle;
};
// ====================================================== { END OF FILE } ====================================================== //