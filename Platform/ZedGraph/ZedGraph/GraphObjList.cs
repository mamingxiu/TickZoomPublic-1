//============================================================================
//ZedGraph Class Library - A Flexible Line Graph/Bar Graph Library in C#
//Copyright � 2004  John Champion
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Drawing;
using System.Collections.Generic;
using TickZoom.Api;

namespace ZedGraph
{
	/// <summary>
	/// A collection class containing a list of <see cref="TextObj"/> objects
	/// to be displayed on the graph.
	/// </summary>
	/// 
	/// <author> John Champion </author>
	/// <version> $Revision: 3.1 $ $Date: 2006/06/24 20:26:44 $ </version>
	[Serializable]
	public class GraphObjList : ICloneable
	{
	    private List<GraphObj> list = new List<GraphObj>();
	    private List<List<GraphObj>> zOrderList = new List<List<GraphObj>>();

	    public int Count
	    {
            get { return list.Count;  }
	    }
	
		public void Add(GraphObj value)
		{
		    list.Add(value);
		}

	    public GraphObj this[int index]
	    {
            get { return list[index]; }
            set
            {
                list[index] = value;
            }
	    }

	#region Constructors
		/// <summary>
		/// Default constructor for the <see cref="GraphObjList"/> collection class
		/// </summary>
		public GraphObjList()
		{
			for( int i=0; i<(int)ZOrder.MaxValue; i++) {
				zOrderList.Add( new List<GraphObj>());
			}
		}

		/// <summary>
		/// The Copy Constructor
		/// </summary>
		/// <param name="rhs">The <see cref="GraphObjList"/> object from which to copy</param>
		public GraphObjList( GraphObjList rhs )
		{
            for (var i = 0; i < rhs.list.Count; i++ )
            {
                var item = rhs.list[i];
                list.Add((GraphObj)((ICloneable)item).Clone());
            }
		}

		/// <summary>
		/// Implement the <see cref="ICloneable" /> interface in a typesafe manner by just
		/// calling the typed version of <see cref="Clone" />
		/// </summary>
		/// <returns>A deep copy of this object</returns>
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// Typesafe, deep-copy clone method.
		/// </summary>
		/// <returns>A new, independent copy of this class</returns>
		public GraphObjList Clone()
		{
			return new GraphObjList( this );
		}

		
	#endregion

	#region Methods
/*
		/// <summary>
		/// Indexer to access the specified <see cref="GraphObj"/> object by its ordinal
		/// position in the list.
		/// </summary>
		/// <param name="index">The ordinal position (zero-based) of the
		/// <see cref="GraphObj"/> object to be accessed.</param>
		/// <value>A <see cref="GraphObj"/> object reference.</value>
		public GraphObj this[ int index ]  
		{
			get { return( (GraphObj) graphObjs[index] ); }
			set { graphObjs[index] = value; }
		}
*/		
		/// <summary>
		/// Indexer to access the specified <see cref="GraphObj"/> object by its <see cref="GraphObj.Tag"/>.
		/// Note that the <see cref="GraphObj.Tag"/> must be a <see cref="String"/> type for this method
		/// to work.
		/// </summary>
		/// <param name="tag">The <see cref="String"/> type tag to search for.</param>
		/// <value>A <see cref="GraphObj"/> object reference.</value>
		/// <seealso cref="IndexOfTag"/>
		public GraphObj this[string tag]  
		{
			get
			{
				int index = IndexOfTag( tag );
				if ( index >= 0 )
					return( this[index]  );
				else
					return null;
			}
		}
/*		
		/// <summary>
		/// Add a <see cref="GraphObj"/> object to the <see cref="GraphObjList"/>
		/// collection at the end of the list.
		/// </summary>
		/// <param name="item">A reference to the <see cref="GraphObj"/> object to
		/// be added</param>
		/// <seealso cref="IList.Add"/>
		public GraphObj Add( GraphObj item )
		{
			graphObjs.Add( item );
			return item;
		}

		/// <summary>
		/// Insert a <see cref="GraphObj"/> object into the collection
		/// at the specified zero-based index location.
		/// </summary>
		/// <param name="index">The zero-based index location for insertion.</param>
		/// <param name="item">The <see cref="GraphObj"/> object that is to be
		/// inserted.</param>
		/// <seealso cref="IList.Insert"/>
		public void Insert( int index, GraphObj item )
		{
			graphObjs.Insert( index, item );
		}
*/
		/// <summary>
		/// Return the zero-based position index of the
		/// <see cref="GraphObj"/> with the specified <see cref="GraphObj.Tag"/>.
		/// </summary>
		/// <remarks>In order for this method to work, the <see cref="GraphObj.Tag"/>
		/// property must be of type <see cref="String"/>.</remarks>
		/// <param name="tag">The <see cref="String"/> tag that is in the
		/// <see cref="GraphObj.Tag"/> attribute of the item to be found.
		/// </param>
		/// <returns>The zero-based index of the specified <see cref="GraphObj"/>,
		/// or -1 if the <see cref="GraphObj"/> is not in the list</returns>
		public int IndexOfTag( string tag )
		{
			int index = 0;
            for (var i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p.Tag is string &&
                            String.Compare((string)p.Tag, tag, true) == 0)
                    return index;
                index++;
            }

			return -1;
		}
		
		/// <summary>
		/// Move the position of the object at the specified index
		/// to the new relative position in the list.</summary>
		/// <remarks>For Graphic type objects, this method controls the
		/// Z-Order of the items.  Objects at the beginning of the list
		/// appear in front of objects at the end of the list.</remarks>
		/// <param name="index">The zero-based index of the object
		/// to be moved.</param>
		/// <param name="relativePos">The relative number of TradeSignal.Positions to move
		/// the object.  A value of -1 will move the
		/// object one position earlier in the list, a value
		/// of 1 will move it one position later.  To move an item to the
		/// beginning of the list, use a large negative value (such as -999).
		/// To move it to the end of the list, use a large positive value.
		/// </param>
		/// <returns>The new position for the object, or -1 if the object
		/// was not found.</returns>
		public int Move( int index, int relativePos )
		{
			if ( index < 0 || index >= list.Count )
				return -1;

			GraphObj graphObj = this[index];
			list.RemoveAt( index );

			index += relativePos;
			if ( index < 0 )
				index = 0;
			if ( index > list.Count )
				index = list.Count;

			list.Insert( index, graphObj );
			return index;
		}

	#endregion

	#region Render Methods
	public void OptimizeDrawing() {
		for( int i = 0; i<zOrderList.Count; i++) {
			zOrderList[i].Clear();
		}
		for( var i=0; i< list.Count; i++)
		{
		    var graphObj = list[i];
			zOrderList[(int)graphObj.ZOrder].Add(graphObj);
		}
	}
	
	private MultiDimBitArray isPixelDrawn = null;

		/// <summary>
		/// Render text to the specified <see cref="Graphics"/> device
		/// by calling the Draw method of each <see cref="GraphObj"/> object in
		/// the collection.
		/// </summary>
		/// <remarks>This method is normally only called by the Draw method
		/// of the parent <see cref="GraphPane"/> object.
		/// </remarks>
		/// <param name="g">
		/// A graphic device object to be drawn into.  This is normally e.Graphics from the
		/// PaintEventArgs argument to the Paint() method.
		/// </param>
		/// <param name="pane">
		/// A reference to the <see cref="PaneBase"/> object that is the parent or
		/// owner of this object.
		/// </param>
		/// <param name="scaleFactor">
		/// The scaling factor to be used for rendering objects.  This is calculated and
		/// passed down by the parent <see cref="GraphPane"/> object using the
		/// <see cref="PaneBase.CalcScaleFactor"/> method, and is used to proportionally adjust
		/// font sizes, etc. according to the actual size of the graph.
		/// </param>
		/// <param name="zOrder">A <see cref="ZOrder"/> enumeration that controls
		/// the placement of this <see cref="GraphObj"/> relative to other
		/// graphic objects.  The order of <see cref="GraphObj"/>'s with the
		/// same <see cref="ZOrder"/> value is control by their order in
		/// this <see cref="GraphObjList"/>.</param>
		public void Draw( Graphics g, PaneBase pane, float scaleFactor,
							ZOrder zOrder )
		{
			GraphPane graphPane = pane as GraphPane;
			int minX = int.MinValue;
			int maxX = int.MaxValue;
			int minY = int.MinValue;
			int maxY = int.MaxValue;
			bool isOptDraw = false;
			if( graphPane != null) {
				isOptDraw = true;
				minX = (int)graphPane.Chart.Rect.Left;
				maxX = (int)graphPane.Chart.Rect.Right;
				minY = (int)graphPane.Chart.Rect.Top;
				maxY = (int)graphPane.Chart.Rect.Bottom;
				if ( isOptDraw ) {
					if( isPixelDrawn == null)
					{
					    isPixelDrawn = new MultiDimBitArray(maxX,maxY);
					} else
					{
					    isPixelDrawn.TryResize(maxX,maxY);
					}
				}
			}
			
			
			
			// Draw the items in reverse order, so the last items in the
			// list appear behind the first items (consistent with
			// CurveList)
			List<GraphObj> graphObjs = zOrderList[(int)zOrder];
			for ( int i=graphObjs.Count-1; i>=0; i-- )
			{
				GraphObj item = graphObjs[i];
				PointF pix = item.Location.Transform(pane);
				if ( item.ZOrder == zOrder && item.IsVisible &&
				     pix.X >= minX && pix.X <= maxX &&
				     pix.Y >= minY && pix.Y <= maxY)
				{
					// Don't try to draw where we already drew.
					// This is a huge optimization when there are
					// many more draw items than pixels in the rectangle.
					if ( isPixelDrawn[(int)pix.X,(int)pix.Y] )
						continue;
					isPixelDrawn[(int)pix.X,(int)pix.Y] = true;
					
					Region region = null;
					if ( item.IsClippedToChartRect && pane is GraphPane )
					{
						region = g.Clip.Clone();
						g.SetClip( ((GraphPane)pane).Chart._rect );
					}

					item.Draw( g, pane, scaleFactor );

					if ( item.IsClippedToChartRect && pane is GraphPane )
						g.Clip = region;
				}
			}
		}
		
		/// <summary>
		/// Determine if a mouse point is within any <see cref="GraphObj"/>, and if so, 
		/// return the index number of the the <see cref="GraphObj"/>.
		/// </summary>
		/// <param name="mousePt">The screen point, in pixel coordinates.</param>
		/// <param name="pane">
		/// A reference to the <see cref="PaneBase"/> object that is the parent or
		/// owner of this object.
		/// </param>
		/// <param name="g">
		/// A graphic device object to be drawn into.  This is normally e.Graphics from the
		/// PaintEventArgs argument to the Paint() method.
		/// </param>
		/// <param name="scaleFactor">
		/// The scaling factor to be used for rendering objects.  This is calculated and
		/// passed down by the parent <see cref="GraphPane"/> object using the
		/// <see cref="PaneBase.CalcScaleFactor"/> method, and is used to proportionally adjust
		/// font sizes, etc. according to the actual size of the graph.
		/// </param>
		/// <param name="index">The index number of the <see cref="TextObj"/>
		///  that is under the mouse point.  The <see cref="TextObj"/> object is
		/// accessible via the <see cref="GraphObjList" /> indexer property.
		/// </param>
		/// <returns>true if the mouse point is within a <see cref="GraphObj"/> bounding
		/// box, false otherwise.</returns>
		/// <seealso cref="GraphPane.FindNearestObject"/>
		public bool FindPoint( PointF mousePt, PaneBase pane, Graphics g, float scaleFactor, out int index, Predicate<object> predicate)
		{
			index = -1;
			// Search in reverse direction to honor the Z-order))
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate != null && !predicate(this[i]))
                {
                    continue;
                }
                if (this[i].PointInBox(mousePt, pane, g, scaleFactor))
                {
                    if ((index >= 0 && this[i].ZOrder > this[index].ZOrder) || index < 0)
                    {
                        index = i;
                        break;
                    }
                }
            }
			if ( index >= 0 )
				return true;
			else
				return false;
		}

        public bool FindLinkableObject(out object source, out Link link, out int index, Predicate<GraphObj> predicate)
        {
            index = -1;
            if( predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            // First look for graph objects that lie in front of the data points
            for (var i = 0; i < list.Count; i++)
            {
                var graphObj = list[i];
                link = graphObj._link;
                bool inFront = graphObj.IsInFrontOfData;

                if (link.IsActive)
                {
                    if (predicate(graphObj))
                    {
                        source = graphObj;
                        index = i;
                        return true;
                    }
                }
            }

            source = null;
            link = null;
            index = -1;
            return false;

        }

	#endregion
	}
}