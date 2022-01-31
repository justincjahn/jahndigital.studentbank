using System.Threading.Tasks;

namespace jahndigital.studentbank.services.Interfaces
{
    public interface IShareTypeService
    {
        /// <summary>
        ///     Reset the withdrawal limit of a given ShareType ID.
        /// </summary>
        /// <param name="shareTypeId"></param>
        /// <returns></returns>
        Task<bool> ResetWithdrawalLimit(long shareTypeId);
    }
}
