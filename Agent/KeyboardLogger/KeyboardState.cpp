// ====================================================== { START OF FILE } ====================================================== //
#include "KeyboardState.h"

void KeyboardState::updateModifier(
    DWORD virtualKey,
    bool down) noexcept
{
    switch (virtualKey)
    {
    case VK_LSHIFT:
        leftShift_ = down;
        break;

    case VK_RSHIFT:
        rightShift_ = down;
        break;

    case VK_LCONTROL:
        leftCtrl_ = down;
        break;

    case VK_RCONTROL:
        rightCtrl_ = down;
        break;

    case VK_LMENU:
        leftAlt_ = down;
        break;

    case VK_RMENU:
        rightAlt_ = down;
        break;

    case VK_LWIN:
        leftWin_ = down;
        break;

    case VK_RWIN:
        rightWin_ = down;
        break;

    default:
        break;
    }
}

bool KeyboardState::shiftDown() const noexcept
{
    return leftShift_ || rightShift_;
}

bool KeyboardState::ctrlDown() const noexcept
{
    return leftCtrl_ || rightCtrl_;
}

bool KeyboardState::altDown() const noexcept
{
    return leftAlt_ || rightAlt_;
}

bool KeyboardState::winDown() const noexcept
{
    return leftWin_ || rightWin_;
}

bool KeyboardState::isPressed(
    DWORD physicalKey) const noexcept
{
    return pressedKeys_.find(physicalKey) != pressedKeys_.end();
}

void KeyboardState::press(
    DWORD physicalKey)
{
    pressedKeys_.insert(physicalKey);
}

void KeyboardState::release(
    DWORD physicalKey)
{
    pressedKeys_.erase(physicalKey);
}
// ====================================================== { END OF FILE } ====================================================== //