// =========================================================================================
// Author:          Sailen Mondal
// Create Date:     2026-11-03
// Description:     View model for the admin product catalog and inventory management dashboard.
// -----------------------------------------------------------------------------------------
// Revision History:
// Date         Author          Description
// ----------   -------------   ------------------------------------------------------------
// 2026-11-03   Sailen Mondal   Initial creation.
// =========================================================================================

using System.Collections.Generic;

namespace FoodDeliveryApp.Models.ViewModels
{
    /// <summary>
    /// View model for managing restaurant menu items and prices in the admin portal.
    /// </summary>
    public class AdminMenuItemViewModel
    {
        /// <summary>
        /// Collection of all food items in the database across all availability states.
        /// </summary>
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Alias collection for menu items.
        /// </summary>
        public List<MenuItem> MenuItems
        {
            get => Items;
            set => Items = value;
        }

        /// <summary>
        /// Collection of all active menu categories.
        /// </summary>
        public List<Category> Categories { get; set; } = new List<Category>();

        /// <summary>
        /// Optional status feedback message.
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
