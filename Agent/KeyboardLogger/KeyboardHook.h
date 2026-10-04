// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include "KeyboardEvent.h"

#include <Windows.h>

#include <functional>

// Owns the Windows low-level keyboard hook.
class KeyboardHook
{
public:
    using Callback =
        std::function<void(const KeyboardEvent &)>;

    explicit KeyboardHook(Callback callback);

    ~KeyboardHook();

    KeyboardHook(const KeyboardHook &) = delete;
    KeyboardHook &operator=(const KeyboardHook &) = delete;

    // Installs the Windows keyboard hook.
    [[nodiscard]]
    bool start();

    // Removes the hook.
    void stop();

    [[nodiscard]]
    bool isRunning() const noexcept;

private:
    // Windows requires a static callback function.
    static LRESULT CALLBACK hookProc(
        int nCode,
        WPARAM wParam,
        LPARAM lParam);

    // This is the actual hook callback that receives events from Windows.
    void handleEvent(
        const KeyboardEvent &event);

private:
    HHOOK hook_{};

    Callback callback_;

    // Only one hook can be active at a time, so we keep a pointer to the active hook.
    static KeyboardHook *activeHook_;
};

// ====================================================== { END OF FILE } ====================================================== //