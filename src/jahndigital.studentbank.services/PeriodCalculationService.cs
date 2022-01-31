using System;

namespace jahndigital.studentbank.services
{
    /// <summary>
    ///     Calculates dates representing <see cref="jahndigital.studentbank.dal.Enums.Period" /> values.
    /// </summary>
    public static class PeriodCalculationService
    {
        /// <summary>
        ///     Returns the next Monday.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime NextWeekly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        /// <summary>
        ///     Returns the previous Monday.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime PreviousWeekly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            do
            {
                date = date.AddDays(-1);
            } while (date.DayOfWeek != DayOfWeek.Monday);

            return date;
        }

        /// <summary>
        ///     Returns the last day of the month.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime NextMonthly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        /// <summary>
        ///     Returns the last day of the previous month.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime PreviousMonthly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;
            date = date.AddMonths(-1);

            return NextMonthly(date);
        }

        /// <summary>
        ///     Returns the last day of the quarter.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime NextQuarterly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            return date
                .AddDays(1 - date.Day)
                .AddMonths(3 - ((date.Month - 1) % 3))
                .AddDays(-1);
        }

        /// <summary>
        ///     Returns the last day of the previous quarter.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime PreviousQuarterly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            return NextMonthly(NextQuarterly(date).AddMonths(-3));
        }

        /// <summary>
        ///     Returns the last day of the year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime NextYearly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            return new DateTime(date.Year, 1, 1).AddYears(1).AddDays(-1);
        }

        /// <summary>
        ///     Returns the last day of the previous year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public static DateTime PreviousYearly(DateTime? startDate = null)
        {
            DateTime date = startDate ?? DateTime.Today;

            return new DateTime(date.Year - 1, 1, 1).AddYears(1).AddDays(-1);
        }
    }
}
