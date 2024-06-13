﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Luna.Data;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
namespace Luna.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class SendEmailConfirmController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly AppDbContext _dbContext;

        public SendEmailConfirmController(IEmailSender emailSender, AppDbContext dbContext)
        {
            _emailSender = emailSender;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var bill = _dbContext.GetBills(9).FirstOrDefault();
            return View(bill);
        }

        [HttpPost]
        public async Task<IActionResult> SendMail()
        {
            await _emailSender.SendEmailAsync("hatran3072003@gmail.com", "Xác nhận đặt phòng",
                        GetOrderDetailsFromDatabase(9));
            return View("Index");
        }
        private string GetOrderDetailsFromDatabase(int orderId)
        {
            var bill = _dbContext.GetBills(orderId).FirstOrDefault();
            string formattedString = $"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color:#ffffff;margin:10px 0;max-width:700px\">\r\n    <tbody>\r\n        <tr>\r\n            <td width=\"35\">&nbsp;</td>\r\n            <td width=\"630\" style=\"text-align:center\">\r\n                <a href=\"https://www.heritagehotelnyc.com\" target=\"_blank\" data-saferedirecturl=\"https://www.google.com/url?q=https://www.heritagehotelnyc.com&amp;source=gmail&amp;ust=1717655976367000&amp;usg=AOvVaw1F2xhqDa_4H5cZ9Qcsyhf3\"><img src=\"https://scontent.xx.fbcdn.net/v/t1.15752-9/445985217_492552843124379_4649829859454233095_n.jpg?stp=dst-jpg_p206x206&_nc_cat=102&ccb=1-7&_nc_sid=5f2048&_nc_eui2=AeFc5EryMFx9lnnPNyxC9znBlewyWuVh8M6V7DJa5WHwzpG5haNekObbDSWB7DFyOl_5qBEWvYtBRgb5rlVQw6Om&_nc_ohc=whOJ2MdJpXcQ7kNvgHpQFXS&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_Q7cD1QGSd4dvGcmp1TVMjj7iWWyWlWyFZ6hHzRFYJldQLWOJXQ&oe=66921D7F\" alt=\"The Heritage Hotel New York City - 18 W 25th Street, New York City, NY 10010, USA\" title=\"The Heritage Hotel New York City - 18 W 25th Street, New York City, NY 10010, USA\" class=\"CToWUd\" data-bit=\"iit\"></a>\r\n\r\n            </td>\r\n            <td width=\"35\">&nbsp;</td>\r\n        </tr>\r\n\r\n        <tr>\r\n            <td width=\"35\">&nbsp;</td>\r\n\r\n            <td width=\"630\" style=\"border:3px solid #dddddd\">\r\n                <table width=\"630\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                    <tbody>\r\n                        <tr>\r\n                            <td width=\"25\">&nbsp;</td>\r\n\r\n                            <td width=\"580\">\r\n                                <table width=\"580\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                    <tbody>\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><h1 style=\"font-family:Arial,sans-serif;font-weight:700;text-align:center;text-transform:uppercase;font-size:27px;letter-spacing:2px;border-bottom:1px solid #000000\">Reservation Confirmation</h1></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><p style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;margin:0\">Dear <strong>{bill.UserName},</strong> </p></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td><p style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;margin:0;line-height:25px\">Thank you for your reservation made through INNsight.com at <strong>Hotel Del Luna</strong> checking in {bill.Checkin}</p></td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>&nbsp;</td>\r\n                                        </tr>\r\n\r\n                                        <tr>\r\n                                            <td>\r\n                                                <table width=\"580\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n                                                    <tbody>\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Confirmation Details</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Reservation ID:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\"><strong>{bill.OrderId}</strong></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Booking Source:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Website</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Name:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.FullName}</span></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Phone:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.PhoneNumber}</span></td>\r\n                                                        </tr>\r\n                                                        <tr>\r\n                                                            <td width=\"150\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Your Email:</span></td>\r\n                                                            <td width=\"430\"><span style=\"font-family:Arial,sans-serif;font-size:15px;line-height:25px;display:inline-block\"><i><a href=\"mailto:hatran3072003@gmail.com\" style=\"color:#555\" target=\"_blank\">{bill.Email}</a></i></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Property Information</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Property Name:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\"><strong>Hotel Del Luna</strong></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td valign=\"top\" width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Address:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\">\r\n                                                                <span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">\r\n                                                                    Ngũ Hành Sơn, Đà Nẵng, Việt Nam\r\n                                                                </span>\r\n                                                            </td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Phone:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\">\r\n                                                                <span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">\r\n                                                                    +1 (212) 645-3990\r\n                                                                </span>\r\n                                                            </td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Email:</span></td>\r\n                                                            <td width=\"430\"><span style=\"font-family:Arial,sans-serif;font-size:15px;line-height:25px;display:inline-block\"><i><a href=\"mailto:hatran3072003@gmail.com\" style=\"color:#555\" target=\"_blank\">hoteldelluna@gmail.com</a></i></span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\" style=\"padding:10px;background-color:#eee\"><h2 style=\"text-align:center;font-family:Arial,sans-serif;font-size:20px;letter-spacing:0.5px;color:#555;font-weight:500;margin:0\">Booking Details</h2></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Arrival:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.Checkin}</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Departure:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.Checkout}</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">No. of Rooms:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.TotalRoom} </span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td width=\"150\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">Total price:</span></td>\r\n                                                            <td width=\"430\" style=\"padding-bottom:8px\"><span style=\"font-family:Arial,sans-serif;font-size:15px;color:#555;line-height:25px;display:inline-block\">{bill.Deposits}</span></td>\r\n                                                        </tr>\r\n\r\n                                                        <tr>\r\n                                                            <td colspan=\"2\">&nbsp;</td>\r\n                                                        </tr>\r\n\r\n                                                    </tbody>\r\n                                                </table>\r\n                                            </td>\r\n                                        </tr>\r\n\r\n                                    </tbody>\r\n                                </table>\r\n                            </td>\r\n\r\n                            <td width=\"25\">&nbsp;</td>\r\n\r\n                        </tr>\r\n\r\n                    </tbody>\r\n                </table>\r\n\r\n            </td>\r\n\r\n            <td width=\"35\">&nbsp;</td>\r\n\r\n        </tr>\r\n\r\n    </tbody>\r\n</table>";
            return formattedString;
        }
    }
}
