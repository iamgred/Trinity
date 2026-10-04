// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include "IntentEvent.h"
#include "KeyboardHook.h"
#include "KeyboardInterpreter.h"
#include "KeyboardState.h"

#include <span>
#include <vector>

// High-level keyboard intent recorder.
class InputIntentRecorder
{
public:
    InputIntentRecorder();

    ~InputIntentRecorder();

    // Recording owns state and resources, so copying is disabled.
    InputIntentRecorder(const InputIntentRecorder &) = delete;

    InputIntentRecorder &operator=(
        const InputIntentRecorder &) = delete;

    // Starts recording keyboard events.
    [[nodiscard]]
    bool start();

    // Stops recording.
    void stop();

    // Returns true while the keyboard hook is active.
    [[nodiscard]]
    bool isRunning() const noexcept;

    // Returns all recorded semantic events.
    [[nodiscard]]
    std::span<const IntentEvent> events() const noexcept;

    // Removes all recorded events.
    void clear();

private:
    // Called by KeyboardHook for every keyboard event.
    void onKeyboardEvent(
        const KeyboardEvent &event);

private:
    // Tracks which keys/modifiers are currently held.
    KeyboardState state_;

    // Converts keyboard input into semantic events.
    KeyboardInterpreter interpreter_;

    // Owns the actual Windows keyboard hook.
    KeyboardHook hook_;

    // Recorded semantic events.
    std::vector<IntentEvent> events_;
};

// ====================================================== { END OF FILE } ====================================================== //