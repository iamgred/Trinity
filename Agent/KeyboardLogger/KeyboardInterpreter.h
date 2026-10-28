// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include "IntentEvent.h"
#include "KeyboardEvent.h"
#include "KeyboardState.h"

#include <Windows.h>

#include <string>

// Converts low-level keyboard events into semantic IntentEvent objects.
class KeyboardInterpreter
{
public:
    // Converts a low-level keyboard event into a semantic event.
    [[nodiscard]]
    IntentEvent interpret(
        const KeyboardEvent &event,
        const KeyboardState &state) const;

private:
    // Obtains the keyboard layout currently active for the foreground thread/window.
    [[nodiscard]]
    HKL getActiveKeyboardLayout() const;

    // Converts a key event into Unicode text.
    [[nodiscard]]
    std::wstring translateToText(
        const KeyboardEvent &event,
        const KeyboardState &state,
        HKL keyboardLayout) const;

    // Gets a human-readable name for a key.
    [[nodiscard]]
    std::wstring getKeyName(
        const KeyboardEvent &event,
        HKL keyboardLayout) const;

    // Produces the modifier portion of a shortcut.
    [[nodiscard]]
    std::wstring getModifierPrefix(
        const KeyboardState &state) const;

    // Determines whether Caps Lock is enabled.
    [[nodiscard]]
    bool capsLockOn() const noexcept;

    // Determines whether Num Lock is enabled.
    [[nodiscard]]
    bool numLockOn() const noexcept;

    // Determines whether Scroll Lock is enabled.
    [[nodiscard]]
    bool scrollLockOn() const noexcept;

    // Converts a key into its semantic meaning.
    [[nodiscard]]
    IntentEvent interpretKey(
        const KeyboardEvent &event,
        const KeyboardState &state,
        HKL keyboardLayout) const;
};
// ====================================================== { END OF FILE } ====================================================== //