using Microsoft.AspNetCore.Http;
using TechShare.Models;

namespace TechShare.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(HttpContext context, VNPaymentRequestModel model);
        VNPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
