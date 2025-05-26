using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElCentre.Core.Entities;

namespace ElCentre.Core.Services
{
    public interface IPaymobService
    {
        Task<(Enrollment Enrollment, string RedirectUrl)> ProcessPaymentForOrderAsync(int enrollmentId, string paymentMethod);
        string GetPaymentIframeUrl(string paymentToken);
        string GetMobileWalletPaymentUrl(string paymentToken, string phoneNumber);
        Task<Enrollment> UpdateOrderSuccess(string paymentIntentId);
        Task<Enrollment> UpdateOrderFailed(string paymentIntentId);
        string ComputeHmacSHA512(string data, string secret);
    }

}
