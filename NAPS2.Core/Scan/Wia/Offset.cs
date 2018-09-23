// --------------------------------------------------------------------------------
//  <copyright file="Offset.cs" company="NAPS2 Development Team">
//     Copyright 2012-2018 Ben Olden-Cooligan and contributors. All rights reserved.   
//  </copyright>
// --------------------------------------------------------------------------------

namespace NAPS2.Scan.Wia
{
    /// <summary>
    ///     Defines offsets from a rectangle boundary.
    /// </summary>
    public class Offset
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Offset" /> class.
        /// </summary>
        /// <param name="top">
        ///     The top offset.
        /// </param>
        /// <param name="bottom">
        ///     The bottom offset.
        /// </param>
        /// <param name="left">
        ///     The left offset.
        /// </param>
        /// <param name="right">
        ///     The right offset.
        /// </param>
        public Offset(int top = 0, int bottom = 0, int left = 0, int right = 0)
        {
            this.Top = top;
            this.Bottom = bottom;
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        ///     Gets or sets the bottom offset.
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is empty; otherwise <c>false</c>.
        /// </value>
        public bool IsEmpty => this.Top == 0 && this.Bottom == 0 && this.Left == 0 && this.Right == 0;

        /// <summary>
        ///     Gets or sets the left offset.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        ///     Gets or sets the right offset.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        ///     Gets or sets the top offset.
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        ///     Appends the specified increment offset.
        /// </summary>
        /// <param name="incrementOffset">The increment offset.</param>
        /// <returns>An new offset with the increment appended.</returns>
        public Offset Append(Offset incrementOffset)
        {
            return new Offset(
                this.Top + incrementOffset.Top,
                this.Bottom + incrementOffset.Bottom,
                this.Left + incrementOffset.Left,
                this.Right + incrementOffset.Right);
        }

        /// <summary>
        ///     Sets all offsets to zero.
        /// </summary>
        public void Clear()
        {
            this.Top = 0;
            this.Bottom = 0;
            this.Left = 0;
            this.Right = 0;
        }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns>Clone of this instance.</returns>
        public Offset Clone()
        {
            return new Offset(this.Top, this.Bottom, this.Left, this.Right);
        }

        /// <summary>
        ///     Scales the offsets with given x and y scales.
        /// </summary>
        /// <param name="xScale">The x scale.</param>
        /// <param name="yScale">The y scale.</param>
        /// <returns>Scaled offset.</returns>
        public Offset Scale(double xScale, double yScale)
        {
            return new Offset(
                (int)(this.Top * yScale),
                (int)(this.Bottom * yScale),
                (int)(this.Left * xScale),
                (int)(this.Right * xScale));
        }

        /// <summary>
        ///     Scales the offset with a given scale value.
        /// </summary>
        /// <param name="scale">The scale value.</param>
        /// <returns>Scaled offset.</returns>
        public Offset Scale(double scale)
        {
            return this.Scale(scale, scale);
        }
    }
}