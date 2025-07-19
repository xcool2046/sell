using System;
using System.Collections.Generic;
using System.Linq;

namespace Sellsys.WpfClient.Services
{
    /// <summary>
    /// 事件总线，用于模块间通信
    /// </summary>
    public class EventBus
    {
        private static readonly Lazy<EventBus> _instance = new(() => new EventBus());
        public static EventBus Instance => _instance.Value;

        private readonly Dictionary<Type, List<object>> _subscribers = new();

        private EventBus() { }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<object>();
            }
            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish<T>(T eventData)
        {
            var eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                var handlers = _subscribers[eventType].Cast<Action<T>>().ToList();
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler(eventData);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't stop other handlers
                        System.Diagnostics.Debug.WriteLine($"Error in event handler: {ex.Message}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 客户分配事件
    /// </summary>
    public class CustomerAssignedEvent
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
        public int? SupportPersonId { get; set; }
        public string? SupportPersonName { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignmentType { get; set; } = string.Empty; // "Sales" or "Support"
    }

    /// <summary>
    /// 客户更新事件
    /// </summary>
    public class CustomerUpdatedEvent
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string UpdateType { get; set; } = string.Empty; // "Created", "Updated", "Deleted"
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 导航事件
    /// </summary>
    public class NavigationEvent
    {
        public string ModuleName { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
