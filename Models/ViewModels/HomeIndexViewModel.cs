// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model backing the Home landing page with categories and bestsellers.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;
using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// View model for the customer landing page.
    /// </summary>
    public class HomeIndexViewModel
    {
        /// <summary>
        /// Gets or sets top food categories.
        /// </summary>
        public List<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// Gets or sets featured bestseller dishes.
        /// </summary>
        public List<MenuItem> Bestsellers { get; set; } = new List<MenuItem>();
    }
}
