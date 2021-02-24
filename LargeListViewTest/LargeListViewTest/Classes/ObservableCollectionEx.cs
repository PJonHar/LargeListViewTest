//
//	Author:		Phil J Pearson
//  Created:	15 January 2014
//	Last mod:	16 April 2020 12:16:27
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Windows.Threading;

namespace LargeListViewTest.Classes
{
    /// <summary>
    /// Extends <see cref="ObservableCollection<T>"/> to provide methods for adding a range of items or replacing the whole collection
    /// with a range of items and raising only a single <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private bool suppressOnCollectionChanged;
        private DispatcherPriority notificationPriority = DispatcherPriority.DataBind;

        public DispatcherPriority NotificationPriority
        {
            get => this.notificationPriority;
            set => this.notificationPriority = value;
        }

        // Override the event so this class can access it
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableCollectionEx()
        {
        }

        public ObservableCollectionEx(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            AddRange(collection);
        }

        /// <summary>
        /// Adds the supplied items to the collection and raises a single <see cref="CollectionChanged"/> event
        /// when the operation is complete.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<T> items, bool notifyAfter = true)
        {
            if (null == items)
            {
                throw new ArgumentNullException("items");
            }
            if (items.Any())
            {
                try
                {
                    SuppressChangeNotification();
                    CheckReentrancy();
                    foreach (var item in items)
                    {
                        Add(item);
                    }
                }
                finally
                {
                    if (notifyAfter)
                        FireChangeNotification();
                    suppressOnCollectionChanged = false;
                }
            }
        }

        /// <summary>
        /// Replaces the content of the collection with the supplied items and raises a single <see cref="CollectionChanged"/> event
        /// when the operation is complete.
        /// </summary>
        /// <param name="items">The items to replace the current content.</param>
        public void ReplaceContent(IEnumerable<T> items)
        {
            SuppressChangeNotification();
            ClearItems();
            AddRange(items);
        }

        public void ReplaceContentWithoutNotification(IEnumerable<T> items)
        {
            SuppressChangeNotification();
            ClearItems();
            AddRange(items, false);
        }

        public void SuppressChangeNotification()
        {
            suppressOnCollectionChanged = true;
        }

        public void FireChangeNotification()
        {
            suppressOnCollectionChanged = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!suppressOnCollectionChanged)
            {
#if NoCrossThreadSupport
				base.OnCollectionChanged(e);
#else
                using (BlockReentrancy())
                {
                    NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                    if (eventHandler == null)
                        return;

                    Delegate[] delegates = eventHandler.GetInvocationList();

                    // Walk the invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        DispatcherObject dispatcherObject = handler.Target as DispatcherObject;

                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                        {
                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
                        }
                        else // Execute handler as is
                            handler(this, e);
                    }
                }
#endif
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!suppressOnCollectionChanged)
                base.OnPropertyChanged(e);
        }
    }
}