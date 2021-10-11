﻿/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;

using Loxodon.Log;
#if NETFX_CORE
using System.Reflection;
#endif

namespace Loxodon.Framework.Messaging
{
    /// <summary>
    /// The Messenger is a class allowing objects to exchange messages.
    /// </summary>
    public class Messenger : IMessenger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Messenger));

        public static readonly Messenger Default = new Messenger();

        private readonly Dictionary<Type, SubjectBase> notifiers = new Dictionary<Type, SubjectBase>();
        private readonly Dictionary<string, Dictionary<Type, SubjectBase>> channelNotifiers = new Dictionary<string, Dictionary<Type, SubjectBase>>();

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <param name="type">The type of message that the recipient subscribes for.</param>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public virtual IDisposable Subscribe(Type type, Action<object> action)
        {
            SubjectBase notifier;
            lock (notifiers)
            {
                if (!notifiers.TryGetValue(type, out notifier))
                {
                    notifier = new Subject<object>();
                    notifiers.Add(type, notifier);
                }
            }
            return (notifier as Subject<object>).Subscribe(action);
        }

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <typeparam name="T">The type of message that the recipient subscribes for.</typeparam>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public virtual IDisposable Subscribe<T>(Action<T> action)
        {
            SubjectBase notifier;
            lock (notifiers)
            {
                if (!notifiers.TryGetValue(typeof(T), out notifier))
                {
                    notifier = new Subject<T>();
                    notifiers.Add(typeof(T), notifier);
                }
            }
            return (notifier as Subject<T>).Subscribe(action);
        }

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="type">The type of message that the recipient subscribes for.</param>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public virtual IDisposable Subscribe(string channel, Type type, Action<object> action)
        {
            Dictionary<Type, SubjectBase> dict = null;
            SubjectBase notifier = null;
            lock (channelNotifiers)
            {
                if (!channelNotifiers.TryGetValue(channel, out dict))
                {
                    dict = new Dictionary<Type, SubjectBase>();
                    channelNotifiers.Add(channel, dict);
                }

                if (!dict.TryGetValue(type, out notifier))
                {
                    notifier = new Subject<object>();
                    dict.Add(type, notifier);
                }
            }
            return (notifier as Subject<object>).Subscribe(action);
        }

        /// <summary>
        /// Subscribe a message.
        /// </summary>
        /// <typeparam name="T">The type of message that the recipient subscribes for.</typeparam>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="action">The action that will be executed when a message of type T is sent.</param>
        /// <returns>Disposable object that can be used to unsubscribe the message from the messenger.
        /// if the disposable object is disposed,the message is automatically unsubscribed.</returns>
        public virtual IDisposable Subscribe<T>(string channel, Action<T> action)
        {
            Dictionary<Type, SubjectBase> dict = null;
            SubjectBase notifier = null;
            lock (channelNotifiers)
            {
                if (!channelNotifiers.TryGetValue(channel, out dict))
                {
                    dict = new Dictionary<Type, SubjectBase>();
                    channelNotifiers.Add(channel, dict);
                }

                if (!dict.TryGetValue(typeof(T), out notifier))
                {
                    notifier = new Subject<T>();
                    dict.Add(typeof(T), notifier);
                }
            }
            return (notifier as Subject<T>).Subscribe(action);
        }

        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <param name="message"></param>
        public virtual void Publish(object message)
        {
            this.Publish<object>(message);
        }

        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <typeparam name="T">The type of message that will be sent.</typeparam>
        /// <param name="message">The message to send to subscribed recipients.</param>
        public virtual void Publish<T>(T message)
        {
            if (message == null)
                return;

            Type messageType = message.GetType();

            List<KeyValuePair<Type, SubjectBase>> list;

            lock (notifiers)
            {
                if (notifiers.Count <= 0)
                    return;

                list = new List<KeyValuePair<Type, SubjectBase>>(this.notifiers);
            }

            foreach (KeyValuePair<Type, SubjectBase> kv in list)
            {
                try
                {
                    if (kv.Key.IsAssignableFrom(messageType))
                        kv.Value.Publish(message);
                }
                catch (Exception e)
                {
                    if (log.IsWarnEnabled)
                        log.Warn(e);
                }
            }
        }

        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="message">The message to send to subscribed recipients.</param>
        public virtual void Publish(string channel, object message)
        {
            this.Publish<object>(channel, message);
        }

        /// <summary>
        /// Publish a message to subscribed recipients. 
        /// </summary>
        /// <typeparam name="T">The type of message that will be sent.</typeparam>
        /// <param name="channel">A name for a messaging channel.If a recipient subscribes
        /// using a channel, and a sender sends a message using the same channel, then this
        /// message will be delivered to the recipient. Other recipients who did not
        /// use a channel when subscribing (or who used a different channel) will not
        /// get the message. </param>
        /// <param name="message">The message to send to subscribed recipients.</param>
        public virtual void Publish<T>(string channel, T message)
        {
            if (string.IsNullOrEmpty(channel) || message == null)
                return;

            Type messageType = message.GetType();
            Dictionary<Type, SubjectBase> dict = null;
            List<KeyValuePair<Type, SubjectBase>> list = null;

            lock (this.channelNotifiers)
            {
                if (!channelNotifiers.TryGetValue(channel, out dict) || dict.Count <= 0)
                    return;

                list = new List<KeyValuePair<Type, SubjectBase>>(dict);
            }

            foreach (KeyValuePair<Type, SubjectBase> kv in list)
            {
                try
                {
                    if (kv.Key.IsAssignableFrom(messageType))
                        kv.Value.Publish(message);
                }
                catch (Exception e)
                {
                    if (log.IsWarnEnabled)
                        log.Warn(e);
                }
            }
        }
    }
}
