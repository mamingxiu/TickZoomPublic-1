#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

using TickZoom.Api;

/// <summary>
/// Description of ElapsedTypeConverter.
/// </summary>
public class ElapsedTypeConverter : TimeSpanConverter
{
    // Methods
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
    }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (value is string)
        {
            string s = ((string) value).Trim();
            try
            {
            	TimeSpan span = TimeSpan.Parse(s);
            	return new Elapsed(span.Hours,span.Minutes,span.Seconds);
            }
            catch (FormatException exception)
            {
                throw new FormatException("ConvertInvalidPrimitive = Elapsed" + exception);
            }
        }
        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == null)
        {
            throw new ArgumentNullException("destinationType");
        }
        if ((destinationType == typeof(InstanceDescriptor)) && (value is Elapsed))
        {
        	MethodInfo method = GetType().GetMethod("Parse", new Type[] { typeof(string) });
            if (method != null)
            {
                return new InstanceDescriptor(method, new object[] { value.ToString() });
            }
        }
        if ((destinationType == typeof(string)) && (value is Elapsed))
        {
        	Elapsed elapsed = (Elapsed) value;
        	TimeSpan span = new TimeSpan( elapsed.Hours, elapsed.Minutes, elapsed.Seconds);
        	return span.ToString();
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
    
    public Elapsed Parse(string text) {
    	TimeSpan span = TimeSpan.Parse(text);
    	return  new Elapsed(span.Hours,span.Minutes,span.Seconds);
    }
}
