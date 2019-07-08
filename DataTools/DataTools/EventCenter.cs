using System;
using System.Collections.Generic;

namespace DataTools
{
    public delegate void CallBack();
    public delegate void CallBack<T>(T arg);
    public delegate void CallBack<T1, T2>(T1 arg1, T2 arg2);
    public delegate void CallBack<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void CallBack<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void CallBack<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    /// <summary>
    /// 广播事件处理中心
    /// </summary>
    public class EventCenter
    {
        private static Dictionary<Enum, Delegate> m_EventTable = new Dictionary<Enum, Delegate>();

        /// <summary>
        /// 添加监听检查
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        private static void OnAddListenerCheck(Enum eventCode, Delegate callBack)
        {
            if (!m_EventTable.ContainsKey(eventCode))
                m_EventTable.Add(eventCode, null);

            Delegate d = m_EventTable[eventCode];
            if (d != null && d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("添加监听错误:尝试为事件码{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托类型为{2}"
                    , eventCode, d.GetType(), callBack.GetType()));
            }

        }

        /// <summary>
        /// 移除监听检查
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        private static void OnRemoveListenerCheck(Enum eventCode, Delegate callBack)
        {
            if (m_EventTable.ContainsKey(eventCode))
            {
                Delegate d = m_EventTable[eventCode];
                if (d == null)
                {
                    throw new Exception(string.Format("移除监听错误：事件码{0}没有对应的委托", eventCode));
                }
                else if (d.GetType() != callBack.GetType())
                {
                    throw new Exception(string.Format("移除监听错误：尝试为事件码{0}移除不同类型的委托，当前委托类型为{1}，要移除的委托类型为{2}"
                        , eventCode, d.GetType(), callBack.GetType()));
                }
            }
            else
            {
                throw new Exception(string.Format("移除监听错误：没有事件码{0}", eventCode));
            }
        }

        /// <summary>
        /// 清除空事件码
        /// </summary>
        /// <param name="eventCode">事件码</param>
        private static void OnListenerRemoved(Enum eventCode)
        {
            if (m_EventTable[eventCode] == null)
                m_EventTable.Remove(eventCode);
        }

        /// <summary>
        /// 无参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener(Enum eventCode, CallBack callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 1个参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener<T>(Enum eventCode, CallBack<T> callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T>)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 2个参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener<T, T1>(Enum eventCode, CallBack<T, T1> callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1>)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 3个参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener<T, T1, T2>(Enum eventCode, CallBack<T, T1, T2> callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2>)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 4个参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener<T, T1, T2, T3>(Enum eventCode, CallBack<T, T1, T2, T3> callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2, T3>)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 5个参数添加监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void AddListener<T, T1, T2, T3, T4>(Enum eventCode, CallBack<T, T1, T2, T3, T4> callBack)
        {
            OnAddListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2, T3, T4>)m_EventTable[eventCode] + callBack;
        }

        /// <summary>
        /// 无参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener(Enum eventCode, CallBack callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 1个参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener<T>(Enum eventCode, CallBack<T> callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T>)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 2个参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener<T, T1>(Enum eventCode, CallBack<T, T1> callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1>)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 3个参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener<T, T1, T2>(Enum eventCode, CallBack<T, T1, T2> callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2>)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 4个参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener<T, T1, T2, T3>(Enum eventCode, CallBack<T, T1, T2, T3> callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2, T3>)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 5个参数移除监听
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="callBack">委托内容</param>
        public static void RemoveListener<T, T1, T2, T3, T4>(Enum eventCode, CallBack<T, T1, T2, T3, T4> callBack)
        {
            OnRemoveListenerCheck(eventCode, callBack);
            m_EventTable[eventCode] = (CallBack<T, T1, T2, T3, T4>)m_EventTable[eventCode] - callBack;
            OnListenerRemoved(eventCode);
        }

        /// <summary>
        /// 无参数广播委托
        /// </summary>
        /// <param name="eventCode">事件码</param>
        public static void Broadcast(Enum eventCode)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack callBack = d as CallBack;
                if (callBack != null)
                    callBack();
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }

        /// <summary>
        /// 1个参数广播委托
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="eventCode">事件码</param>
        /// <param name="arg">参数</param>
        public static void Broadcast<T>(Enum eventCode, T arg)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack<T> callBack = d as CallBack<T>;
                if (callBack != null)
                    callBack(arg);
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }

        /// <summary>
        /// 2个参数广播委托
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        public static void Broadcast<T, T1>(Enum eventCode, T arg1, T1 arg2)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack<T, T1> callBack = d as CallBack<T, T1>;
                if (callBack != null)
                    callBack(arg1, arg2);
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }

        /// <summary>
        /// 3个参数广播委托
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        /// <param name="arg3">参数3</param>
        public static void Broadcast<T, T1, T2>(Enum eventCode, T arg1, T1 arg2, T2 arg3)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack<T, T1, T2> callBack = d as CallBack<T, T1, T2>;
                if (callBack != null)
                    callBack(arg1, arg2, arg3);
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }

        /// <summary>
        /// 4个参数广播委托
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        /// <param name="arg3">参数3</param>
        /// <param name="arg4">参数4</param>
        public static void Broadcast<T, T1, T2, T3>(Enum eventCode, T arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack<T, T1, T2, T3> callBack = d as CallBack<T, T1, T2, T3>;
                if (callBack != null)
                    callBack(arg1, arg2, arg3, arg4);
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }

        /// <summary>
        /// 5个参数广播委托
        /// </summary>
        /// <param name="eventCode">事件码</param>
        /// <param name="arg1">参数1</param>
        /// <param name="arg2">参数2</param>
        /// <param name="arg3">参数3</param>
        /// <param name="arg4">参数4</param>
        /// <param name="arg5">参数5</param>
        public static void Broadcast<T, T1, T2, T3, T4>(Enum eventCode, T arg1, T1 arg2, T2 arg3, T3 arg4, T4 arg5)
        {
            Delegate d;
            if (m_EventTable.TryGetValue(eventCode, out d))
            {
                CallBack<T, T1, T2, T3, T4> callBack = d as CallBack<T, T1, T2, T3, T4>;
                if (callBack != null)
                    callBack(arg1, arg2, arg3, arg4, arg5);
                else
                    throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventCode));
            }
        }
    }
}