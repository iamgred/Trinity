// ====================================================== { START OF FILE } ====================================================== //
#include "InputIntentRecorder.h"

#include <Windows.h>

#include <utility>

InputIntentRecorder::InputIntentRecorder()
    : hook_(
          [this](const KeyboardEvent &event)
          {
              onKeyboardEvent(event);
          })
{
}

InputIntentRecorder::~InputIntentRecorder()
{
    stop();
}

bool InputIntentRecorder::start()
{
    return hook_.start();
}

void InputIntentRecorder::stop()
{
    hook_.stop();
}

bool InputIntentRecorder::isRunning() const noexcept
{
    return hook_.isRunning();
}

std::span<const IntentEvent>
InputIntentRecorder::events() const noexcept
{
    return events_;
}

void InputIntentRecorder::clear()
{
    events_.clear();
}

void InputIntentRecorder::onKeyboardEvent(
    const KeyboardEvent &event)
{
    if (event.isInjected())
    {
        return;
    }

    const DWORD physicalKey =
        (event.flags & LLKHF_EXTENDED)
            ? (event.scanCode | 0x100)
            : event.scanCode;

    if (event.isKeyDown())
    {
        const bool alreadyPressed =
            state_.isPressed(physicalKey);

        state_.press(physicalKey);

        if (alreadyPressed)
        {
            return;
        }

        state_.updateModifier(
            event.virtualKey,
            true);

        events_.push_back(
            interpreter_.interpret(
                event,
                state_));

        return;
    }

    if (event.isKeyUp())
    {
        state_.release(physicalKey);

        state_.updateModifier(
            event.virtualKey,
            false);
    }
}
// ====================================================== { END OF FILE } ====================================================== //