﻿// Copyright 2014 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of SharpMap.Rendering.Thematics.CategoryTheme.
// SharpMap.Rendering.Decoration.Legend is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Rendering.Thematics.CategoryTheme is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Styles.Thematics
{
	///<summary>
	///</summary>
	[Serializable]
	public class CategoryTheme<T>: Style, IThemeStyle, IStyle
        where T: IComparable<T>
	{
		private readonly List<ICategoryThemeItem<T>> _items = new List<ICategoryThemeItem<T>> ();
		
		private string _columnName;
		
		private bool _sorted;
		
		public bool UseDefaultStyleForDbNull;

        /// <summary>
        /// Creates an instance of this class setting the default style to a disabled
        /// </summary>
	    public CategoryTheme()
            :this(CreateDisabledStyle())
	    {
	    }

        /// <summary>
        /// Creates an instance of this class setting the default style
        /// </summary>
        /// <param name="style">The default style</param>
        public CategoryTheme(IStyle style)
        {
            _default = style;
        }

        /// <summary>
        /// The column name to get the value from
        /// </summary>
        public string ColumnName
		{
			get
			{
				return _columnName;
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException("value");
				if (value.Equals(_columnName))
					return;
				
				_columnName = value.ToUpper();
				//this.OnPropertyChanged("ColumnName");
			}
		}
		
		private IStyle _default;
		
		/// <summary>
		/// Gets or sets the default <see cref="IStyle"/>, used when the attribute value does not match the criteria
		/// </summary>
		public IStyle Default
		{
			get { return _default ?? (_default = CreateDisabledStyle()); }
			set
			{
				_default = value;
				//this.OnPropertyChanged("Default");
			}
		}

	    private static IStyle CreateDisabledStyle()
	    {
	        var res = new VectorStyle();
	        res.Enabled = false;
	        return res;
	    }

		///<summary>
		///</summary>
		///<param name="cti"></param>
		public void Add(ICategoryThemeItem<T> cti)
		{
			_items.Add(cti);
			_sorted = false;
			//this.OnPropertyChanged("Items");
		}
		
		///<summary>
		///</summary>
		public void Clear()
		{
			_items.Clear();
			//this.OnPropertyChanged("Items");
		}
		
		/// <summary>
		/// Method to evaluate the style
		/// </summary>
		public IStyle GetStyle(IFeature attribute)
		{
			Debug.Assert(!String.IsNullOrEmpty(_columnName));
			Debug.Assert(_default != null);	
			
			if (attribute == null)
				throw new ArgumentNullException("attribute", "The attribute row must not be null!");

            // If the row has no value return the default style
            //If (attribute.IsNull(_columnName)
            if (attribute[_columnName]==null) 
                return UseDefaultStyleForDbNull ? _default : null;
			
			// Sort the list
			if (!_sorted)
			{
				_items.Sort();
				_sorted = true;
			}
			
			object val = attribute[_columnName];
			if (val is T)
			{
				var tval = (T)val;
				try
				{
					var cti = _items.Find(c => c.Matches(tval));
					return cti.Style;
				}
				catch(NullReferenceException)
				{
				}
				//foreach (ICategoryThemeItem<T> categoryThemeItem in _items)
				//{
				//    if (categoryThemeItem.Matches(tval)) return categoryThemeItem.Style;
				//}
			}
			
			return _default;
		}
		
		/// <summary>
		/// Method to expose all <see cref="ICategoryThemeItem{T}"/>s.
		/// </summary>
		/// <returns></returns>
		public System.Collections.ObjectModel.ReadOnlyCollection<ICategoryThemeItem<T>> ItemsAsReadOnly()
		{
			return _items.AsReadOnly();
		}

        IStyle IThemeStyle.GetStyle(IFeature attribute)
        {
            //Debug.Assert(!String.IsNullOrEmpty(_columnName));
            //Debug.Assert(_default != null);

            if (attribute == null)
                throw new ArgumentNullException("feature", "The feature must not be null!");

            // If the row has no value return the default style
            //If (attribute.IsNull(_columnName)
            if (attribute[_columnName] == null)
                return UseDefaultStyleForDbNull ? _default : null;

            // Sort the list
            if (!_sorted)
            {
                _items.Sort();
                _sorted = true;
            }

            object val = attribute[_columnName];
            if (val is T)
            {
                var tval = (T)val;
                try
                {
                    var cti = _items.Find(c => c.Matches(tval));
                    return cti.Style;
                }
                catch (NullReferenceException)
                {
                }
                //foreach (ICategoryThemeItem<T> categoryThemeItem in _items)
                //{
                //    if (categoryThemeItem.Matches(tval)) return categoryThemeItem.Style;
                //}
            }

            return _default;
        }
    }
}
