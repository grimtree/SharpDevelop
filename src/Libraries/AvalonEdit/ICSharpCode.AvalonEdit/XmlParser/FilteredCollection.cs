﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ICSharpCode.AvalonEdit.Xml
{
	/// <summary>
	/// Collection that presents only some items from the wrapped collection.
	/// It implicitely filters object that are not of type T (or derived).
	/// </summary>
	public class FilteredCollection<T, C>: ObservableCollection<T> where C: INotifyCollectionChanged, IList
	{
		C source;
		Predicate<object> condition;
		List<int> srcPtrs = new List<int>(); // Index to the original collection
		
		/// <summary> Wrap the given collection.  Items of type other then T are filtered </summary>
		public FilteredCollection(C source) : this (source, x => true) { }
		
		/// <summary> Wrap the given collection.  Items of type other then T are filtered.  Items not matching the condition are filtered. </summary>
		public FilteredCollection(C source, Predicate<object> condition)
		{
			this.source = source;
			this.condition = condition;
			
			this.source.CollectionChanged += SourceCollectionChanged;
			
			Reset();
		}
		
		void Reset()
		{
			this.Clear();
			srcPtrs.Clear();
			for(int i = 0; i < source.Count; i++) {
				if (source[i] is T && condition(source[i])) {
					this.Add((T)source[i]);
					srcPtrs.Add(i);
				}
			}
		}
	
		void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action) {
				case NotifyCollectionChangedAction.Add:
					// Update pointers
					for(int i = 0; i < srcPtrs.Count; i++) {
						if (srcPtrs[i] >= e.NewStartingIndex) {
							srcPtrs[i] += e.NewItems.Count;
						}
					}
					// Find where to add items
					int addIndex = srcPtrs.FindIndex(srcPtr => srcPtr >= e.NewStartingIndex);
					if (addIndex == -1) addIndex = this.Count;
					// Add items to collection
					for(int i = 0; i < e.NewItems.Count; i++) {
						if (e.NewItems[i] is T && condition(e.NewItems[i])) {
							this.InsertItem(addIndex, (T)e.NewItems[i]);
							srcPtrs.Insert(addIndex, e.NewStartingIndex + i);
							addIndex++;
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					// Remove the item from our collection
					for(int i = 0; i < e.OldItems.Count; i++) {
						// Anyone points to the removed item?
						int removeIndex = srcPtrs.IndexOf(e.OldStartingIndex + i);
						// Remove
						if (removeIndex != -1) {
							this.RemoveAt(removeIndex);
							srcPtrs.RemoveAt(removeIndex);
						}
					}
					// Update pointers
					for(int i = 0; i < srcPtrs.Count; i++) {
						if (srcPtrs[i] >= e.OldStartingIndex) {
							srcPtrs[i] -= e.OldItems.Count;
						}
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
				default:
					throw new NotSupportedException(e.Action.ToString());
			}
		}
	}
}
