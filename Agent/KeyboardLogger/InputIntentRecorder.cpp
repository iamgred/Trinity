// ====================================================== { START OF FILE } ====================================================== //
#include "InputIntentRecorder.h"

#include <Windows.h>

#include <chrono>
#include <cstdint>
#include <string>
#include <utility>

#include "rapidjson/stringbuffer.h"
#include "rapidjson/writer.h"

namespace
{
    std::string WideToUtf8(const std::wstring &value)
    {
        if (value.empty())
        {
            return {};
        }

        const int size =
            WideCharToMultiByte(
                CP_UTF8,
                0,
                value.data(),
                static_cast<int>(value.size()),
                nullptr,
                0,
                nullptr,
                nullptr);

        if (size <= 0)
        {
            return {};
        }

        std::string result(
            size,
            '\0');

        WideCharToMultiByte(
            CP_UTF8,
            0,
            value.data(),
            static_cast<int>(value.size()),
            result.data(),
            size,
            nullptr,
            nullptr);

        return result;
    }
}

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

std::string InputIntentRecorder::events() const
{
    rapidjson::StringBuffer buffer;
    rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);

    writer.StartArray();

    for (const auto &event : events_)
    {
        writer.StartObject();

        writer.Key("timestamp");
        writer.Int64(
            std::chrono::duration_cast<std::chrono::milliseconds>(
                event.timestamp.time_since_epoch())
                .count());

        writer.Key("type");
        switch (event.type)
        {
        case IntentType::Text:
            writer.String("Text");
            break;
        case IntentType::Shortcut:
            writer.String("Shortcut");
            break;
        case IntentType::SpecialKey:
            writer.String("SpecialKey");
            break;
        case IntentType::Modifier:
            writer.String("Modifier");
            break;
        default:
            writer.String("Unknown");
            break;
        }

        writer.Key("meaning");
        writer.String(WideToUtf8(event.meaning).c_str());

        writer.Key("text");
        writer.String(WideToUtf8(event.text).c_str());

        writer.Key("focusedWindow");
        writer.Uint64(reinterpret_cast<uint64_t>(event.focusedWindow));

        writer.Key("focusedProcessId");
        writer.Uint(event.focusedProcessId);

        writer.Key("focusedWindowTitle");
        writer.String(WideToUtf8(event.focusedWindowTitle).c_str());

        writer.EndObject();
    }

    writer.EndArray();
    return buffer.GetString();
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