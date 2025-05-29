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
        Task<(Enrollment Enrollment, string RedirectUrl)> ProcessPaymentForCardAsync(int enrollmentId);
        Task<(Enrollment Enrollment, string RedirectUrl)> ProcessPaymentForWalletAsync(int enrollmentId);
        string GetPaymentIframeUrl(string paymentToken);
        Task<Enrollment> UpdateOrderSuccess(string specialReference);
        Task<Enrollment> UpdateOrderFailed(string specialReference);
        string ComputeHmacSHA512(string data, string secret);
    }

}
