using System;
using System.Linq;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Interface for user interaction
    /// </summary>
    public interface IUserInteraction
    {
        bool ConfirmAction(string message);
    }
}