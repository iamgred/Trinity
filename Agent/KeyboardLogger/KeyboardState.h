// ====================================================== { START OF FILE } ====================================================== //
#pragma once

#include <Windows.h>

#include <unordered_set>

// Maintains the current state of the keyboard.
class KeyboardState
{
public:
    // Updates modifier state when a modifier is pressed/released.
    void updateModifier(
        DWORD virtualKey,
        bool down) noexcept;

    // Returns true if either Shift key is currently pressed.
    [[nodiscard]]
    bool shiftDown() const noexcept;

    // Returns true if either Ctrl key is currently pressed.
    [[nodiscard]]
    bool ctrlDown() const noexcept;

    // Returns true if either Alt key is currently pressed.
    [[nodiscard]]
    bool altDown() const noexcept;

    // Returns true if either Windows key is currently pressed.
    [[nodiscard]]
    bool winDown() const noexcept;

    // Returns true if the specified physical key is currently pressed.
    [[nodiscard]]
    bool isPressed(DWORD physicalKey) const noexcept;

    // Marks a physical key as pressed.
    void press(DWORD physicalKey);

    // Marks a physical key as released.
    void release(DWORD physicalKey);

private:
    // Scan codes of keys currently held down.
    std::unordered_set<DWORD> pressedKeys_;

    // Left/right modifiers are kept separately.
    bool leftShift_{};
    bool rightShift_{};

    bool leftCtrl_{};
    bool rightCtrl_{};

    bool leftAlt_{};
    bool rightAlt_{};

    bool leftWin_{};
    bool rightWin_{};
};
// ====================================================== { END OF FILE } ====================================================== //