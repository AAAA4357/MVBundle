using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Windows;

namespace MVPlot.Managers
{
    /// <summary>
    /// 窗口管理器
    /// </summary>
    public static class WindowManager
    {
        static Hashtable _RegisterWindows = [];

        /// <summary>
        /// 注册窗口
        /// </summary>
        /// <param name="key">窗口名称</param>
        /// <param name="window">窗口实例</param>
        public static void Register(string key, Window window)
        {
            if (!_RegisterWindows.Contains(key))
            {
                _RegisterWindows.Add(key, window);
            }
        }

        /// <summary>
        /// 移除窗口
        /// </summary>
        /// <param name="key">窗口名称</param>
        public static void Remove(string key)
        {
            if (_RegisterWindows.ContainsKey(key))
            {
                _RegisterWindows.Remove(key);
            }
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="key">窗口名称</param>
        public static void Show(string key)
        {
            if (_RegisterWindows.ContainsKey(key))
            {
                ((Window)_RegisterWindows[key]!).Show();
            }
        }

        /// <summary>
        /// 显示窗口（强制获取焦点）
        /// </summary>
        /// <param name="key">窗口名称</param>
        public static void ShowDialog(string key)
        {
            if (_RegisterWindows.ContainsKey(key))
            {
                ((Window)_RegisterWindows[key]!).ShowDialog();
            }
        }

        /// <summary>
        /// 隐藏窗口
        /// </summary>
        /// <param name="key">窗口名称</param>
        public static void Hide(string key)
        {
            if (_RegisterWindows.ContainsKey(key))
            {
                ((Window)_RegisterWindows[key]!).Hide();
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="key">窗口名称</param>
        public static void Close(string key)
        {
            if (_RegisterWindows.ContainsKey(key))
            {
                ((Window)_RegisterWindows[key]!).Close();
                Remove(key);
            }
        }
    }
}