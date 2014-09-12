using System;

namespace BacktestingSoftware
{
    /// <summary>
    /// This class represents a bar. In trading a bar is a "sum" of all ticks that have been active over the specified time period. This application only supports<br/>
    /// minute bars at the moment. Therefore every bar stored in a bar-object by this class will be a minute bar. A bar is specified by 4 main values and a timestamp<br/>
    /// that tells when the bar ended. The first of the main values is the open value. This one is the value at the beginning of the bars time period.<br/>
    /// The second one is the high value. This one is the highest value that could be documented in the bars time period. The low value is the lowest value<br/>
    /// in the bars time period. The last value is the last avlue at the end of the time period when the bar "closes".
    /// </summary>
    /// <remarks></remarks>
    internal class Bar
    {
        /// <summary>
        /// Gets the time stamp of the bar.
        /// </summary>
        /// <remarks></remarks>
        public DateTime TimeStamp { get; private set; }

        /// <summary>
        /// Gets the open value of the bar.
        /// </summary>
        /// <remarks></remarks>
        public decimal Open { get; private set; }

        /// <summary>
        /// Gets the high value of the bar.
        /// </summary>
        /// <remarks></remarks>
        public decimal High { get; private set; }

        /// <summary>
        /// Gets the low value of the bar.
        /// </summary>
        /// <remarks></remarks>
        public decimal Low { get; private set; }

        /// <summary>
        /// Gets the close value of the bar.
        /// </summary>
        /// <remarks></remarks>
        public decimal Close { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bar"/> class representing a bar. This constructor only stores the parameters that were handed over into the<br/>
        /// attributes.
        /// </summary>
        /// <param name="timeStamp">The time stamp.</param>
        /// <param name="open">The open value.</param>
        /// <param name="high">The high value.</param>
        /// <param name="low">The low value.</param>
        /// <param name="close">The close value.</param>
        /// <remarks></remarks>
        public Bar(DateTime timeStamp, decimal open, decimal high, decimal low, decimal close)
        {
            this.TimeStamp = timeStamp;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
        }
    }
}