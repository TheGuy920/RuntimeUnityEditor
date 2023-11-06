﻿using System;
using System.Collections;
using HarmonyLib;

namespace Plasma.Mods.RuntimeUnityEditor.Core.Inspector.Entries
{
    public class ListCacheEntry : CacheEntryBase
    {
        private Type _type;
        private readonly IList _list;
        private readonly int _index;

        public ListCacheEntry(IList container, int index) : base(ReadonlyListCacheEntry.GetListItemName(index), $"Item contained inside of a list.\n\nIndex: {index}\n\nList type: {container.GetType().FullDescription()}", null)
        {
            _index = index;
            _list = container;
        }

        public override object GetValueToCache()
        {
            return _list.Count > _index ? _list[_index] : "ERROR: The list was changed while browsing!";
        }

        protected override bool OnSetValue(object newValue)
        {
            if (CanSetValue())
            {
                _list[_index] = newValue;
                _type = null;
                return true;
            }

            return false;
        }

        public override Type Type()
        {
            return _type ?? (_type = GetValue()?.GetType());
        }

        public override bool CanSetValue()
        {
            return !_list.IsReadOnly;
        }
    }
}
