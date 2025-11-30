using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;
using GBSWarehouse.Helpers;
using GBSWarehouse.Models;
using GBSWarehouse.Models.Dtos;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using JWT.Serializers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace GBSWarehouse.Controllers
{
    [ApiController]
    [Route("api/GBSWarehouse")]
    public class GBSWarehouseController : ControllerBase
    {
        #region Others
        GBSWarehouseContext DBContext = new();
        [Route("[action]")]
        [HttpGet]
        public object ClearData(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                string[,] Inparam = new string[,] { };
                string[,] Outparam = new string[,] { };

                DBClass.CallStoredProcedure(Inparam, Outparam, "ClearData");
                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = "Cleared successfully!";
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [HttpGet]
        public object Get()
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = "API Version No: " + DBContext.Versions.Select(x => x.Apiversion).FirstOrDefault();

                return new { responseStatus };
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = "A network problem has occurred. Please contact your network administrator";

                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        public static int? DecryptToken(string token)
        {
            if (token == null) return null;
            RijndaelCrypt rijndaelCrypt = new RijndaelCrypt();
            token = token.Replace(" ", "");
            const string secret = "GQDstcKsx0NHjPOdAgtBVertHRYeaFDAsSEaGTbeJ1XT0uFiwDVvVBrk";
            try
            {
                var DecToken = rijndaelCrypt.Decrypt(token);
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var json = decoder.Decode(DecToken, secret, verify: true);
                JavaScriptSerializer ser = new JavaScriptSerializer();
                var jsonObj = ser.Deserialize<Token>(json);
                return jsonObj.userId;

            }
            catch (TokenExpiredException)
            {
                return -1;// "Token has expired";
            }
            catch (SignatureVerificationException)
            {
                return -2;// "Token has invalid signature";
            }
            catch (Exception)
            {
                return -3;//Exception error
            }
        }
        #endregion
        #region User Settings 
        [Route("[action]")]
        [HttpGet]
        public object GetMobileVersion(string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                var MobileVersionNo = DBContext.Versions.Select(x => x.FrondendVersion).FirstOrDefault();

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Getting data successfully");

                return new { responseStatus, MobileVersionNo };
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetByUserID(long UserID, string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            Helpers.User user = new Helpers.User();
            try
            {
                if (!DBContext.Users.Any(x => x.UserId == UserID))
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "معرف المستخدم خاطئ" : "User ID is wrong!");
                    return new { responseStatus, user };
                }
                else
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Getting data successfully");

                    user = DBContext.Users.Where(x => x.UserId == UserID).Select(x => new Helpers.User
                    {
                        UserId = x.UserId,
                        UserName = x.UserName,
                        SapUserCode = x.SapUserCode,
                        Pass = EncryptionManager.Decrypt(x.Password),
                        RoleId = x.RoleId ?? 0,
                        IsMobileApp = x.IsMobileOffice ?? false,
                        IsBackOfficeApp = x.IsBackOffice ?? false,
                        IsActive = x.IsActive ?? false
                    }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, user };

        }
        [Route("[action]")]
        [HttpGet]
        public object SignIn(string UserName, string Pass, string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            UserInfo userInfo = new UserInfo();
            double MobileVersionNo = 0;
            try
            {
                MobileVersionNo = DBContext.Versions.Select(x => x.FrondendVersion).FirstOrDefault().Value;
                var user = DBContext.Users.Where(x => x.UserName == UserName && x.Password == EncryptionManager.Encrypt(Pass)).FirstOrDefault();

                if (user == null)
                {
                    if (!DBContext.Users.Any(x => x.UserName == UserName))
                    {
                        responseStatus.StatusCode = 400;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "إسم المستخدم خطأ" : "Wrong username!");
                        return new { responseStatus, userInfo, MobileVersionNo };
                    }

                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كلمة المرور خاطئة" : "Wrong password!");

                }
                else
                {
                    if ((bool)user.IsActive != true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "حساب المستخدم مغلق.يرجى التوجه لمسؤول النظام" : "User account is locked contact your system administrator!");
                        return new { responseStatus, userInfo, MobileVersionNo };
                    }


                    if ((bool)user.IsMobileOffice == false)
                    {
                        responseStatus.StatusCode = 402;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "حساب المستخدم غير مصرح له بالوصول إلى تطبيق الهاتف المحمول" : "User account is not authorized to access the mobile application!");
                        return new { responseStatus, userInfo, MobileVersionNo };
                    }

                    userInfo.UserId = user.UserId;
                    userInfo.SapUserCode = user.SapUserCode;
                    userInfo.user = user;

                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم تسجيل الدخول بنجاح" : "You are logged in successfully");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, userInfo, MobileVersionNo };
        }
        [Route("[action]")]
        [HttpGet]
        public object BackofficeSignIn(string UserName, string Pass, string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            UserInfo userInfo = new UserInfo();
            try
            {
                var user = DBContext.Users.Where(x => x.UserName == UserName && x.Password == EncryptionManager.Encrypt(Pass)).FirstOrDefault();

                if (user == null)
                {
                    if (!DBContext.Users.Any(x => x.UserName == UserName))
                    {
                        responseStatus.StatusCode = 400;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "إسم المستخدم خطأ" : "Wrong username!");
                        return new { responseStatus, userInfo };
                    }

                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كلمة المرور خاطئة" : "Wrong password!");

                }
                else
                {
                    if ((bool)user.IsActive != true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "حساب المستخدم مغلق.يرجى التوجه لمسؤول النظام" : "User account is locked contact your system administrator!");
                        return new { responseStatus, userInfo };
                    }


                    if ((bool)user.IsBackOffice == false)
                    {
                        responseStatus.StatusCode = 402;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "حساب المستخدم غير مصرح له بالوصول إلى تطبيق المكتبي" : "User account is not authorized to access the back office application!");
                        return new { responseStatus, userInfo };
                    }

                    userInfo.UserId = user.UserId;
                    userInfo.SapUserCode = user.SapUserCode;
                    userInfo.user = user;

                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم تسجيل الدخول بنجاح" : "You are logged in successfully");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, userInfo };
        }
        [Route("[action]")]
        [HttpPost]
        public object SapSignIn([FromBody] SignInParam model)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            long UserId = 0;
            const string secret = "GQDstcKsx0NHjPOdAgtBVertHRYeaFDAsSEaGTbeJ1XT0uFiwDVvVBrk";
            try
            {
                var user = DBContext.Users.Where(x => x.UserName == model.UserName && x.Password == EncryptionManager.Encrypt(model.Password)).FirstOrDefault();

                if (user == null)
                {
                    if (!DBContext.Users.Any(x => x.UserName == model.UserName))
                    {
                        responseStatus.StatusCode = 400;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "إسم المستخدم خطأ" : "Wrong username!");
                        return new { responseStatus };
                    }

                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كلمة المرور خاطئة" : "Wrong password!");
                }
                else
                {
                    UserId = user.UserId;
                    if ((bool)user.IsActive != true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "حساب المستخدم مغلق.يرجى التوجه لمسؤول النظام" : "User account is locked contact your system administrator!");
                        return new { responseStatus, UserId };
                    }
                    var token = JwtBuilder.Create()
                      .WithAlgorithm(new HMACSHA256Algorithm()) // symmetric
                      .WithSecret(secret)
                      .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(15).ToUnixTimeSeconds())
                      .AddClaim("userId", user.UserId)
                      .Encode();
                    RijndaelCrypt rijndaelCrypt = new RijndaelCrypt();

                    var EncToken = rijndaelCrypt.Encrypt(token);

                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لقد تم تسجيل الدخول بنجاح" : "You are logged in successfully");
                    return new { responseStatus, EncToken, UserId };
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object ChangePassword(long UserID, string DeviceSerialNo, string OldPass, string NewPass, string applang)
        {
            ResponseStatus responseStatus = new ResponseStatus();
            try
            {
                if (OldPass == NewPass)
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كلمة المرور الجديدة هي نفسها كلمة المرور القديمة" : "The new password is the same as the old password!");
                }
                else
                {
                    var user = DBContext.Users.Where(x => x.UserId == UserID).FirstOrDefault();

                    if (user == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "معرف المستخدم خاطئ" : "Wrong UserID!");
                    }
                    else
                    {
                        if (EncryptionManager.Decrypt(user.Password) != OldPass)
                        {
                            responseStatus.StatusCode = 402;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "كلمة المرور القديمة خاطئة" : "Wrong old password");
                        }
                        else
                        {
                            user.Password = EncryptionManager.Encrypt(NewPass);
                            DBContext.Entry(user).State = EntityState.Modified;
                            DBContext.SaveChanges();

                            responseStatus.StatusCode = 200;
                            responseStatus.IsSuccess = true;
                            responseStatus.StatusMessage = ((applang == "ar") ? "تم تغيير الرقم السري بنجاح" : "Password changed successfully");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.IsSuccess = false;
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }

        #endregion
        #region SAP Team Basic Data Setup
        [Route("[action]")]
        [HttpPost]
        public object SapProductAddList([FromBody] SapProductAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProductParam> GetList = new();
            List<ProductsParam> ErrorLog = new List<ProductsParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.Products)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رمز المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "رمز المصنع خاطئ" : "Wrong plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductCode == null || string.IsNullOrEmpty(item.ProductCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductDesc == null || string.IsNullOrEmpty(item.ProductDesc.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وصف المنتج" : "Missing product description");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductDescAr == null || string.IsNullOrEmpty(item.ProductDescAr.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وصف المنتج بالعربي" : "Missing product arabic description");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.NumeratorforConversionPal == null || string.IsNullOrEmpty(item.NumeratorforConversionPal.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال Numerator for Conversion Pal" : "Missing Numerator for Conversion Pal");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.DenominatorforConversionPal == null || string.IsNullOrEmpty(item.DenominatorforConversionPal.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال Denominator for Conversion Pal" : "Missing Denominator for Conversion Pal");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.NumeratorforConversionPac == null || string.IsNullOrEmpty(item.NumeratorforConversionPac.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال Numerator for Conversion Pac" : "Missing Numerator for Conversion Pac");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.DenominatorforConversionPac == null || string.IsNullOrEmpty(item.DenominatorforConversionPac.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال Denominator for Conversion Pac" : "Missing Denominator for Conversion Pac");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.IsWarehouseLocation == null || string.IsNullOrEmpty(item.IsWarehouseLocation.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال Is Warehouse Location" : "Missing Is Warehouse Location");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (DBContext.Products.Any(x => x.ProductCode == item.ProductCode))
                    {
                        var entity = DBContext.Products.FirstOrDefault(x => x.ProductCode == item.ProductCode);

                        entity.ProductCode = item.ProductCode;
                        entity.ProductDesc = item.ProductDesc;
                        entity.Uom = item.Uom;
                        entity.ProductDescAr = item.ProductDescAr;
                        entity.NumeratorforConversionPal = item.NumeratorforConversionPal;
                        entity.NumeratorforConversionPac = item.NumeratorforConversionPac;
                        entity.DenominatorforConversionPac = item.DenominatorforConversionPac;
                        entity.DenominatorforConversionPal = item.DenominatorforConversionPal;
                        entity.PlantCode = item.PlantCode;
                        entity.IsWarehouseLocation = item.IsWarehouseLocation;

                        DBContext.Products.Update(entity);
                    }
                    else
                    {
                        Product entity = new()
                        {
                            ProductCode = item.ProductCode,
                            ProductDesc = item.ProductDesc,
                            Uom = item.Uom,
                            DenominatorforConversionPal = item.DenominatorforConversionPal,
                            NumeratorforConversionPac = item.NumeratorforConversionPac,
                            ProductDescAr = item.ProductDescAr,
                            DenominatorforConversionPac = item.DenominatorforConversionPac,
                            NumeratorforConversionPal = item.NumeratorforConversionPal,
                            PlantCode = item.PlantCode,
                            IsWarehouseLocation = item.IsWarehouseLocation
                        };
                        DBContext.Products.Add(entity);
                    }
                    DBContext.SaveChanges();
                }

                GetList = (from b in DBContext.Products
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductId = b.ProductId,
                               ProductCode = b.ProductCode,
                               ProductDesc = ((model.applang == "ar") ? b.ProductDescAr : b.ProductDesc),
                               NumeratorforConversionPac = b.NumeratorforConversionPac,
                               DenominatorforConversionPac = b.DenominatorforConversionPac,
                               NumeratorforConversionPal = b.NumeratorforConversionPal,
                               DenominatorforConversionPal = b.DenominatorforConversionPal,
                               Uom = b.Uom,
                               IsWarehouseLocation = b.IsWarehouseLocation
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapPlantAddList([FromBody] SapPlantAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<Plant> GetList = new();
            List<PlantsParam> ErrorLog = new List<PlantsParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.Plants)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رمز المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PlantDesc == null || string.IsNullOrEmpty(item.PlantDesc.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وصف المصنع" : "Missing plant description");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        var entity = DBContext.Plants.FirstOrDefault(x => x.PlantCode == item.PlantCode);

                        entity.PlantCode = item.PlantCode;
                        entity.PlantDesc = item.PlantDesc;

                        DBContext.Plants.Update(entity);
                    }
                    else
                    {

                        Plant entity = new()
                        {
                            PlantCode = item.PlantCode,
                            PlantDesc = item.PlantDesc
                        };
                        DBContext.Plants.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                GetList = (from b in DBContext.Plants
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapProductionLineAddList([FromBody] SapProductionLineAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProductionLineParam> GetList = new();
            List<ProductionLinesParam> ErrorLog = new List<ProductionLinesParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.ProductionLines)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductionLineCode == null || string.IsNullOrEmpty(item.ProductionLineCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductionLineDesc == null || string.IsNullOrEmpty(item.ProductionLineDesc.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال اسم خط الإنتاج" : "Missing Production Line Desc");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductionLineNumber == null || string.IsNullOrEmpty(item.ProductionLineNumber.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رقم خط الإنتاج" : "Missing ProductionLineNumber");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.GroupCode == null || string.IsNullOrEmpty(item.GroupCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المجموعة" : "Missing Group Code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductTypes == null || string.IsNullOrEmpty(item.ProductTypes.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال أنواع المنتجات" : "Missing Product Types");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.TheoriticalcapacityHour == null || string.IsNullOrEmpty(item.TheoriticalcapacityHour.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال ساعة القدرة النظرية" : "Missing Theoretical capacity Hour");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.CapacityUnitofMeasure == null || string.IsNullOrEmpty(item.CapacityUnitofMeasure.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال سعة وحدة القياس" : "Missing Capacity Unit of Measure");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.NumberofHoursPerDay == null || string.IsNullOrEmpty(item.NumberofHoursPerDay.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال عدد الساعات في اليوم" : "Missing Number of Hours Per Day");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.NumberOfDaysPerMonth == null || string.IsNullOrEmpty(item.NumberOfDaysPerMonth.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال عدد الايام في الشهر" : "Missing Number Of Days Per Month");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.CapacityUtilizationRate == null || string.IsNullOrEmpty(item.CapacityUtilizationRate.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال معدل استخدام القدرات" : "Missing Capacity Utilization Rate");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (DBContext.ProductionLines.Any(x => x.ProductionLineCode == item.ProductionLineCode && x.PlantCode == item.PlantCode))
                    {
                        var entity = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == item.ProductionLineCode && x.PlantCode == item.PlantCode);

                        entity.ProductionLineCode = item.ProductionLineCode;
                        entity.PlantCode = item.PlantCode;
                        entity.ProductionLineDesc = item.ProductionLineDesc;
                        entity.ProductionLineNumber = item.ProductionLineNumber;
                        entity.GroupCode = item.GroupCode;
                        entity.ProductTypes = item.ProductTypes;
                        entity.TheoriticalcapacityHour = item.TheoriticalcapacityHour;
                        entity.CapacityUnitofMeasure = item.CapacityUnitofMeasure;
                        entity.NumberofHoursPerDay = item.NumberofHoursPerDay;
                        entity.NumberOfDaysPerMonth = item.NumberOfDaysPerMonth;
                        entity.CapacityUtilizationRate = item.CapacityUtilizationRate;

                        DBContext.ProductionLines.Update(entity);

                    }
                    else
                    {
                        ProductionLine entity = new()
                        {
                            PlantCode = item.PlantCode,
                            ProductionLineCode = item.ProductionLineCode,
                            ProductionLineDesc = item.ProductionLineDesc,
                            ProductionLineNumber = item.ProductionLineNumber,
                            GroupCode = item.GroupCode,
                            ProductTypes = item.ProductTypes,
                            TheoriticalcapacityHour = item.TheoriticalcapacityHour,
                            CapacityUnitofMeasure = item.CapacityUnitofMeasure,
                            NumberofHoursPerDay = item.NumberofHoursPerDay,
                            NumberOfDaysPerMonth = item.NumberOfDaysPerMonth,
                            CapacityUtilizationRate = item.CapacityUtilizationRate
                        };
                        DBContext.ProductionLines.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                GetList = (from b in DBContext.ProductionLines
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductionLineParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductionLineId = b.ProductionLineId,
                               ProductionLineCode = b.ProductionLineCode,
                               ProductionLineDesc = b.ProductionLineDesc,
                               ProductionLineNumber = b.ProductionLineNumber,
                               GroupCode = b.GroupCode,
                               ProductTypes = b.ProductTypes,
                               TheoriticalcapacityHour = b.TheoriticalcapacityHour,
                               CapacityUnitofMeasure = b.CapacityUnitofMeasure,
                               NumberofHoursPerDay = b.NumberofHoursPerDay,
                               NumberOfDaysPerMonth = b.NumberOfDaysPerMonth,
                               CapacityUtilizationRate = b.CapacityUtilizationRate
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpPost]
        public object SapAssignProductToProductionLine([FromBody] SapAssignProductToProductionLineParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProductionLineProductsParam> ErrorLog = new List<ProductionLineProductsParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                foreach (var item in model.Products)
                {
                    if (item.ProductCode == null || string.IsNullOrEmpty(item.ProductCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    var product = DBContext.Products.FirstOrDefault(x => x.ProductCode == item.ProductCode);

                    if (product == null)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductionLineCode == null || string.IsNullOrEmpty(item.ProductionLineCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    var productionLine = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == item.ProductionLineCode);

                    if (productionLine == null)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود خط الإنتاج خاطئ" : "Production line code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (!DBContext.ProductionLineProducts.Any(x => x.ProductionLineId == productionLine.ProductionLineId && x.ProductId == product.ProductId))
                    {
                        ProductionLineProduct entity = new()
                        {
                            ProductId = product.ProductId,
                            ProductionLineId = productionLine.ProductionLineId
                        };
                        DBContext.ProductionLineProducts.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapStorageLocationAddList([FromBody] SapStorageLocationAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<StorageLocationParam> GetList = new();
            List<StorageLocationsParam> ErrorLog = new List<StorageLocationsParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.StorageLocations)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.StorageLocationCode == null || string.IsNullOrEmpty(item.StorageLocationCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المخزن" : "Missing storage location code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.StorageLocationCode && x.PlantCode == item.PlantCode))
                    {
                        var entity = DBContext.StorageLocations.FirstOrDefault(x => x.StorageLocationCode == item.StorageLocationCode && x.PlantCode == item.PlantCode);

                        entity.StorageLocationCode = item.StorageLocationCode;
                        entity.PlantCode = item.PlantCode;
                        entity.StorageLocationDesc = item.StorageLocationDesc;

                        DBContext.StorageLocations.Update(entity);
                    }
                    else
                    {
                        StorageLocation entity = new()
                        {
                            PlantCode = item.PlantCode,
                            StorageLocationCode = item.StorageLocationCode,
                            StorageLocationDesc = item.StorageLocationDesc,
                        };
                        DBContext.StorageLocations.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                GetList = (from b in DBContext.StorageLocations
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new StorageLocationParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               StorageLocationId = b.StorageLocationId,
                               StorageLocationCode = b.StorageLocationCode,
                               StorageLocationDesc = b.StorageLocationDesc
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapLaneAddList([FromBody] SapLaneAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<LaneParam> GetList = new();
            List<LanesParam> ErrorLog = new List<LanesParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.Lanes)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.NoOfPallets == null || string.IsNullOrEmpty(item.NoOfPallets.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال عدد الباليت فى المسار" : "Missing No Of Pallets");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.StorageLocationCode == null || string.IsNullOrEmpty(item.StorageLocationCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المخزن" : "Missing storage location code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.LaneCode == null || string.IsNullOrEmpty(item.LaneCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing Lane Code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.StorageLocationCode && x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره" : "Storage Location Code is wrong or not related to the selected plant");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (DBContext.Lanes.Any(x => x.StorageLocationCode == item.StorageLocationCode && x.PlantCode == item.PlantCode && x.LaneCode == item.LaneCode))
                    {
                        var entity = DBContext.Lanes.FirstOrDefault(x => x.StorageLocationCode == item.StorageLocationCode && x.PlantCode == item.PlantCode && x.LaneCode == item.LaneCode);

                        entity.StorageLocationCode = item.StorageLocationCode;
                        entity.LaneCode = item.LaneCode;
                        entity.PlantCode = item.PlantCode;
                        entity.LaneDesc = item.LaneDesc;
                        entity.NoOfPallets = item.NoOfPallets;

                        DBContext.Lanes.Update(entity);
                    }
                    else
                    {
                        Lane entity = new()
                        {
                            PlantCode = item.PlantCode,
                            StorageLocationCode = item.StorageLocationCode,
                            LaneCode = item.LaneCode,
                            LaneDesc = item.LaneDesc,
                            NoOfPallets = item.NoOfPallets
                        };
                        DBContext.Lanes.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                GetList = (from b in DBContext.Lanes
                           join s in DBContext.StorageLocations on b.StorageLocationCode equals s.StorageLocationCode
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new LaneParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               StorageLocationId = s.StorageLocationId,
                               StorageLocationCode = s.StorageLocationCode,
                               StorageLocationDesc = s.StorageLocationDesc,
                               LaneId = b.LaneId,
                               LaneCode = b.LaneCode,
                               LaneDesc = b.LaneDesc,
                               NoOfPallets = b.NoOfPallets
                           }).Distinct().ToList();

                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapOrderTypeAddList([FromBody] SapOrderTypeAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<OrderTypeParam> GetList = new();
            List<OrderTypesParam> ErrorLog = new List<OrderTypesParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.OrderTypes)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PlantCode != null && !string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                        {
                            item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                            ErrorLog.Add(item);
                            continue;
                        }
                    }
                    if (item.OrderTypeCode == null || string.IsNullOrEmpty(item.OrderTypeCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود نوع الطلب" : "Missing order type code");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (DBContext.OrderTypes.Any(x => x.OrderTypeCode == item.OrderTypeCode))
                    {
                        var entity = DBContext.OrderTypes.FirstOrDefault(x => x.OrderTypeCode == item.OrderTypeCode);

                        entity.OrderTypeCode = item.OrderTypeCode;
                        entity.PlantCode = item.PlantCode;
                        entity.OrderTypeDesc = item.OrderTypeDesc;

                        DBContext.OrderTypes.Update(entity);
                    }
                    else
                    {
                        OrderType entity = new()
                        {
                            PlantCode = item.PlantCode,
                            OrderTypeCode = item.OrderTypeCode,
                            OrderTypeDesc = item.OrderTypeDesc,
                        };
                        DBContext.OrderTypes.Add(entity);
                    }
                    DBContext.SaveChanges();
                }


                GetList = (from b in DBContext.OrderTypes
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode into xDG
                           from x in xDG.DefaultIfEmpty()

                           select new OrderTypeParam
                           {
                               PlantId = x.PlantCode == null ? 0 : x.PlantId,
                               PlantCode = x.PlantCode == null ? string.Empty : x.PlantCode,
                               PlantDesc = x.PlantCode == null ? string.Empty : x.PlantDesc,
                               OrderTypeId = b.OrderTypeId,
                               OrderTypeCode = b.OrderTypeCode,
                               OrderTypeDesc = b.OrderTypeDesc
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapRoutesAddList([FromBody] SapRoutesAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<RouteDetailParam> GetList = new();
            List<RoutesParam> ErrorLog = new List<RoutesParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.Routes)
                {
                    if (item.RouteCode == null || string.IsNullOrEmpty(item.RouteCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing route code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.RouteDesc == null || string.IsNullOrEmpty(item.RouteDesc.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وصف المسار" : "Missing route description");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.Staging == null || string.IsNullOrEmpty(item.Staging.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال المنصه" : "Missing staging");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.DestinationPoint == null || string.IsNullOrEmpty(item.DestinationPoint.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال نقطة الوجهة" : "Missing destination point");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.DeparturePoint == null || string.IsNullOrEmpty(item.DeparturePoint.ToString().Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال نقطة المغادرة" : "Missing departure point");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (DBContext.Routes.Any(x => x.RouteCode == item.RouteCode))
                    {
                        var entity = DBContext.Routes.FirstOrDefault(x => x.RouteCode == item.RouteCode);

                        entity.RouteCode = item.RouteCode;
                        entity.RouteDesc = item.RouteDesc;
                        DBContext.Routes.Update(entity);

                        if (DBContext.RouteDetails.Any(x => x.RouteId == entity.RouteId && x.Staging == item.Staging && x.DeparturePoint == item.DeparturePoint && x.DestinationPoint == item.DestinationPoint)) continue;

                        RouteDetail routeDetail = new()
                        {
                            RouteId = entity.RouteId,
                            Staging = item.Staging,
                            DestinationPoint = item.DestinationPoint,
                            DeparturePoint = item.DeparturePoint
                        };
                        DBContext.RouteDetails.Add(routeDetail);

                    }
                    else
                    {
                        Route entity = new()
                        {
                            RouteCode = item.RouteCode,
                            RouteDesc = item.RouteDesc
                        };
                        DBContext.Routes.Add(entity);
                        DBContext.SaveChanges();

                        RouteDetail routeDetail = new()
                        {
                            RouteId = entity.RouteId,
                            Staging = item.Staging,
                            DestinationPoint = item.DestinationPoint,
                            DeparturePoint = item.DeparturePoint
                        };
                        DBContext.RouteDetails.Add(routeDetail);
                    }
                    DBContext.SaveChanges();
                }

                GetList = (from b in DBContext.Routes
                           join d in DBContext.RouteDetails on b.RouteId equals d.RouteId
                           select new RouteDetailParam
                           {
                               RouteId = b.RouteId,
                               RouteCode = b.RouteCode,
                               RouteDesc = b.RouteDesc,
                               Staging = d.Staging,
                               DeparturePoint = d.DeparturePoint,
                               DestinationPoint = d.DestinationPoint
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapShipmentTypesAddList([FromBody] SapShipmentTypesAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ShipmentTypeDetailParam> GetList = new();
            List<ShipmentTypesParam> ErrorLog = new List<ShipmentTypesParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList, ErrorLog };
                }
                foreach (var item in model.ShipmentTypes)
                {
                    if (item.PlantCode == null || string.IsNullOrEmpty(item.PlantCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ShipmentTypeCode == null || string.IsNullOrEmpty(item.ShipmentTypeCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رمز نوع الشحن" : "Missing shipment type code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ShipmentTypeDesc == null || string.IsNullOrEmpty(item.ShipmentTypeDesc.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وصف نوع الشحن" : "Missing shipment type description");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (DBContext.ShipmentTypes.Any(x => x.ShipmentTypeCode == item.ShipmentTypeCode))
                    {
                        var entity = DBContext.ShipmentTypes.FirstOrDefault(x => x.ShipmentTypeCode == item.ShipmentTypeCode);

                        entity.ShipmentTypeCode = item.ShipmentTypeCode;
                        entity.ShipmentTypeDesc = item.ShipmentTypeDesc;

                        DBContext.ShipmentTypes.Update(entity);

                        var entityPlants = DBContext.ShipmentTypePlants.FirstOrDefault(x => x.ShipmentTypeCode == item.ShipmentTypeCode);
                        entityPlants.ShipmentTypeId = entity.ShipmentTypeId;
                        entityPlants.ShipmentTypeCode = item.ShipmentTypeCode;
                        entityPlants.PlantCode = item.PlantCode;

                        DBContext.ShipmentTypePlants.Update(entityPlants);
                    }
                    else
                    {

                        ShipmentType entity = new()
                        {
                            ShipmentTypeCode = item.ShipmentTypeCode,
                            ShipmentTypeDesc = item.ShipmentTypeDesc
                        };
                        DBContext.ShipmentTypes.Add(entity);
                        DBContext.SaveChanges();

                        ShipmentTypePlant entityPlant = new()
                        {
                            ShipmentTypeId = entity.ShipmentTypeId,
                            ShipmentTypeCode = item.ShipmentTypeCode,
                            PlantCode = item.PlantCode
                        };
                        DBContext.ShipmentTypePlants.Add(entityPlant);

                    }
                    DBContext.SaveChanges();
                }

                GetList = (from b in DBContext.ShipmentTypes
                           join sh in DBContext.ShipmentTypePlants on b.ShipmentTypeCode equals sh.ShipmentTypeCode
                           join p in DBContext.Plants on sh.PlantCode equals p.PlantCode
                           select new ShipmentTypeDetailParam
                           {
                               PlantId = p.PlantId,
                               PlantCode = p.PlantCode,
                               PlantDesc = p.PlantDesc,
                               ShipmentTypeId = b.ShipmentTypeId,
                               ShipmentTypeCode = b.ShipmentTypeCode,
                               ShipmentTypeDesc = b.ShipmentTypeDesc

                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        #endregion
        #region Basic Data Setup
        [Route("[action]")]
        [HttpPost]
        public object GenerateBatchNo([FromBody] GenerateBatchNoParam model)
        {
            ResponseStatus responseStatus = new();
            string batchNo = string.Empty;

            try
            {
                if (model.ProductionLineCode == null || string.IsNullOrEmpty(model.ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                    return new { responseStatus, batchNo };
                }
                var productionLine = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == model.ProductionLineCode);

                if (productionLine == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود خط الإنتاج خاطئ" : "Production line code is wrong");
                    return new { responseStatus, batchNo };
                }

                var entity = DBContext.Batchs.FirstOrDefault(x => x.ProductionLineCode == model.ProductionLineCode && x.Dt.Value.Date == DateTime.Now.Date);
                if (entity == null)
                {
                    Batch batch = new Batch()
                    {
                        ProductionLineCode = model.ProductionLineCode,
                        BatchNo = 1,
                        Dt = DateTime.Now.Date
                    };

                    DBContext.Batchs.Add(batch);

                    batchNo = string.Concat("01", model.ProductionLineCode, DateTime.Now.ToString("DDMMYY"));
                }
                else
                {
                    long No = ((entity.BatchNo ?? 0) + 1);
                    entity.BatchNo = No;

                    DBContext.Batchs.Update(entity);
                    batchNo = string.Concat(No.ToString("00"), model.ProductionLineCode, DateTime.Now.ToString("DDMMYY"));

                }
                DBContext.SaveChanges();


                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, batchNo };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapProductGetList([FromBody] SapProductGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProductParam> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }

                GetList = (from b in DBContext.Products
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductId = b.ProductId,
                               ProductCode = b.ProductCode,
                               ProductDesc = ((model.applang == "ar") ? b.ProductDescAr : b.ProductDesc),
                               Uom = b.Uom,
                               NumeratorforConversionPac = b.NumeratorforConversionPac,
                               DenominatorforConversionPac = b.DenominatorforConversionPac,
                               NumeratorforConversionPal = b.NumeratorforConversionPal,
                               DenominatorforConversionPal = b.DenominatorforConversionPal,
                               IsWarehouseLocation = b.IsWarehouseLocation
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapPlantGetList([FromBody] SapPlantGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<Plant> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }

                GetList = (from b in DBContext.Plants
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapProductionLineGetList([FromBody] SapProductionLineGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProductionLineParam> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }

                GetList = (from b in DBContext.ProductionLines
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductionLineParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductionLineId = b.ProductionLineId,
                               ProductionLineCode = b.ProductionLineCode,
                               ProductionLineDesc = b.ProductionLineDesc,
                               ProductionLineNumber = b.ProductionLineNumber,
                               GroupCode = b.GroupCode,
                               ProductTypes = b.ProductTypes,
                               TheoriticalcapacityHour = b.TheoriticalcapacityHour,
                               CapacityUnitofMeasure = b.CapacityUnitofMeasure,
                               NumberofHoursPerDay = b.NumberofHoursPerDay,
                               NumberOfDaysPerMonth = b.NumberOfDaysPerMonth,
                               CapacityUtilizationRate = b.CapacityUtilizationRate
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapStorageLocationGetList([FromBody] SapStorageLocationGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<StorageLocationParam> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }

                GetList = (from b in DBContext.StorageLocations
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new StorageLocationParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               StorageLocationId = b.StorageLocationId,
                               StorageLocationCode = b.StorageLocationCode,
                               StorageLocationDesc = b.StorageLocationDesc
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapLaneGetList([FromBody] SapLaneGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<LaneParam> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }

                GetList = (from b in DBContext.Lanes
                           join s in DBContext.StorageLocations on b.StorageLocationCode equals s.StorageLocationCode
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new LaneParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               StorageLocationId = s.StorageLocationId,
                               StorageLocationCode = s.StorageLocationCode,
                               StorageLocationDesc = s.StorageLocationDesc,
                               LaneId = b.LaneId,
                               LaneCode = b.LaneCode,
                               LaneDesc = b.LaneDesc,
                               NoOfPallets = b.NoOfPallets
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object SapOrderTypeGetList([FromBody] SapOrderTypeGetListParam model)
        {
            ResponseStatus responseStatus = new();
            List<OrderTypeParam> GetList = new();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {

                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, GetList };
                }


                GetList = (from b in DBContext.OrderTypes
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode into xDG
                           from x in xDG.DefaultIfEmpty()

                           select new OrderTypeParam
                           {
                               PlantId = x.PlantCode == null ? 0 : x.PlantId,
                               PlantCode = x.PlantCode == null ? string.Empty : x.PlantCode,
                               PlantDesc = x.PlantCode == null ? string.Empty : x.PlantDesc,
                               OrderTypeId = b.OrderTypeId,
                               OrderTypeCode = b.OrderTypeCode,
                               OrderTypeDesc = b.OrderTypeDesc
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object RouteGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<RouteDetailParam> GetList = new();
            try
            {
                GetList = (from b in DBContext.Routes
                           join d in DBContext.RouteDetails on b.RouteId equals d.RouteId
                           select new RouteDetailParam
                           {
                               RouteId = b.RouteId,
                               RouteCode = b.RouteCode,
                               RouteDesc = b.RouteDesc,
                               Staging = d.Staging,
                               DeparturePoint = d.DeparturePoint,
                               DestinationPoint = d.DestinationPoint
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetShipmentTypePlant(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ShipmentTypeDetailParam> GetList = new();
            try
            {
                GetList = (from b in DBContext.ShipmentTypes
                           join sh in DBContext.ShipmentTypePlants on b.ShipmentTypeCode equals sh.ShipmentTypeCode
                           join p in DBContext.Plants on sh.PlantCode equals p.PlantCode
                           select new ShipmentTypeDetailParam
                           {
                               PlantId = p.PlantId,
                               PlantCode = p.PlantCode,
                               PlantDesc = p.PlantDesc,
                               ShipmentTypeId = b.ShipmentTypeId,
                               ShipmentTypeCode = b.ShipmentTypeCode,
                               ShipmentTypeDesc = b.ShipmentTypeDesc

                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                    return new { responseStatus, GetList };
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object PalletAdd(long UserID, string DeviceSerialNo, string PalletCode, string applang)
        {
            ResponseStatus responseStatus = new();
            List<string> GetList = new();
            try
            {
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, GetList };
                }
                if (DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, GetList };
                }

                Pallet entity = new()
                {
                    PalletCode = PalletCode
                };
                DBContext.Pallets.Add(entity);
                DBContext.SaveChanges();

                GetList = (from b in DBContext.Pallets
                           select b.PalletCode).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object PalletGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<Pallet> GetList = new();
            try
            {
                GetList = (from b in DBContext.Pallets
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object OrderTypeGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<OrderTypeParam> GetList = new();
            try
            {
                GetList = (from b in DBContext.OrderTypes
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode into xDG
                           from x in xDG.DefaultIfEmpty()

                           select new OrderTypeParam
                           {
                               PlantId = x.PlantCode == null ? 0 : x.PlantId,
                               PlantCode = x.PlantCode == null ? string.Empty : x.PlantCode,
                               PlantDesc = x.PlantCode == null ? string.Empty : x.PlantDesc,
                               OrderTypeId = b.OrderTypeId,
                               OrderTypeCode = b.OrderTypeCode,
                               OrderTypeDesc = b.OrderTypeDesc
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object ProductGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ProductParam> GetList = new();

            try
            {

                GetList = (from b in DBContext.Products
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductId = b.ProductId,
                               ProductCode = b.ProductCode,
                               ProductDesc = ((applang == "ar") ? b.ProductDescAr : b.ProductDesc),
                               Uom = b.Uom,
                               NumeratorforConversionPac = b.NumeratorforConversionPac,
                               DenominatorforConversionPac = b.DenominatorforConversionPac,
                               NumeratorforConversionPal = b.NumeratorforConversionPal,
                               DenominatorforConversionPal = b.DenominatorforConversionPal,
                               IsWarehouseLocation = b.IsWarehouseLocation
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object PlantGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<Plant> GetList = new();

            try
            {

                GetList = (from b in DBContext.Plants
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object ProductionLineGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ProductionLineParam> GetList = new();

            try
            {
                GetList = (from b in DBContext.ProductionLines
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           select new ProductionLineParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               ProductionLineId = b.ProductionLineId,
                               ProductionLineCode = b.ProductionLineCode,
                               ProductionLineDesc = b.ProductionLineDesc,
                               ProductionLineNumber = b.ProductionLineNumber,
                               GroupCode = b.GroupCode,
                               ProductTypes = b.ProductTypes,
                               TheoriticalcapacityHour = b.TheoriticalcapacityHour,
                               CapacityUnitofMeasure = b.CapacityUnitofMeasure,
                               NumberofHoursPerDay = b.NumberofHoursPerDay,
                               NumberOfDaysPerMonth = b.NumberOfDaysPerMonth,
                               CapacityUtilizationRate = b.CapacityUtilizationRate,
                               PlCode = b.PlCode
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object StorageLocationGetListByPlant(long UserID, string DeviceSerialNo, string PlantCode, string applang)
        {
            ResponseStatus responseStatus = new();
            List<StorageLocationParam> GetList = new();
            try
            {
                if (PlantCode != null && !string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        return new { responseStatus, GetList };
                    }
                    GetList = (from b in DBContext.StorageLocations
                               join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                               where x.PlantCode == PlantCode
                               select new StorageLocationParam
                               {
                                   PlantId = x.PlantId,
                                   PlantCode = x.PlantCode,
                                   PlantDesc = x.PlantDesc,
                                   StorageLocationId = b.StorageLocationId,
                                   StorageLocationCode = b.StorageLocationCode,
                                   StorageLocationDesc = b.StorageLocationDesc
                               }).Distinct().ToList();
                    if (GetList != null && GetList.Count > 0)
                    {
                        responseStatus.StatusCode = 200;
                        responseStatus.IsSuccess = true;

                        responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                    }
                    else
                    {
                        responseStatus.StatusCode = 400;
                        responseStatus.IsSuccess = false;

                        responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                    }
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "برجاء ادخال كود المصنع" : "Missing Plant Code");
                }


            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object LaneGetListByPlant(long UserID, string DeviceSerialNo, string PlantCode, string StorageLocationCode, string applang)
        {
            ResponseStatus responseStatus = new();
            List<LaneParam> GetList = new();
            try
            {
                if (PlantCode != null && !string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        return new { responseStatus, GetList };
                    }
                    if (StorageLocationCode != null && !string.IsNullOrEmpty(StorageLocationCode.Trim()))
                    {
                        if (!DBContext.StorageLocations.Any(x => x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode))
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره" : "Storage Location Code is wrong or not related to the selected plant");
                            return new { responseStatus, GetList };
                        }

                        GetList = (from b in DBContext.Lanes
                                   join s in DBContext.StorageLocations on b.StorageLocationCode equals s.StorageLocationCode
                                   join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                                   where b.PlantCode == PlantCode && b.StorageLocationCode == StorageLocationCode
                                   select new LaneParam
                                   {
                                       PlantId = x.PlantId,
                                       PlantCode = x.PlantCode,
                                       PlantDesc = x.PlantDesc,
                                       StorageLocationId = s.StorageLocationId,
                                       StorageLocationCode = s.StorageLocationCode,
                                       StorageLocationDesc = s.StorageLocationDesc,
                                       LaneId = b.LaneId,
                                       LaneCode = b.LaneCode,
                                       LaneDesc = b.LaneDesc,
                                       NoOfPallets = b.NoOfPallets
                                   }).Distinct().ToList();
                        if (GetList != null && GetList.Count > 0)
                        {
                            responseStatus.StatusCode = 200;
                            responseStatus.IsSuccess = true;

                            responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                        }
                        else
                        {
                            responseStatus.StatusCode = 400;
                            responseStatus.IsSuccess = false;

                            responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                        }
                    }
                    else
                    {
                        responseStatus.StatusCode = 400;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "برجاء ادخال كود المخزن" : "Missing Storage Location Code");
                    }
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "برجاء ادخال كود المصنع" : "Missing Plant Code");
                }


            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object PalletLocationGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<PalletLocation> GetList = new();
            try
            {
                GetList = (from b in DBContext.PalletLocations
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object LocationGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<Location> GetList = new();
            try
            {
                GetList = (from b in DBContext.Locations
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object CancelReasonGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<CancelReason> GetList = new();

            try
            {

                GetList = (from b in DBContext.CancelReasons
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object ChangeQtyReasonGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ChangeQtyReason> GetList = new();

            try
            {

                GetList = (from b in DBContext.ChangeQtyReasons
                           select b).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        #endregion
        #region SAP Team Operations
        [Route("[action]")]
        [HttpPost]
        // This API for juhayna team to create product order here. (We need to check if SAP team use it or not???)
        public object ProductionOrdersSapAdd([FromBody] CreateProcessOrderParam model)
        {
            ResponseStatus responseStatus = new();
            List<ProcessOrdersParam> GetList = new();
            try
            {
                if (model.QtyCartoon == null || model.QtyCartoon <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الكمية" : "Missing quantity");
                    return new { responseStatus, GetList };
                }
                if (model.SapOrderId == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال معرف طلب Sap" : "Missing Sap Order Id");
                    return new { responseStatus, GetList };
                }
                if (model.ProductionDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الطلب" : "Missing order date");
                    return new { responseStatus, GetList };
                }
                if (model.BasicFinishDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الانتهاء الأساسي" : "Missing Basic Finish Date");
                    return new { responseStatus, GetList };
                }
                if (model.ProductCode == null || string.IsNullOrEmpty(model.ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus, GetList };
                }
                if (model.PlantCode == null || string.IsNullOrEmpty(model.PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, GetList };
                }
                if (model.ProductionVersion == null || string.IsNullOrEmpty(model.ProductionVersion.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال نسخة الإنتاج" : "Missing Production Version");
                    return new { responseStatus, GetList };
                }
                if (model.BaseUnitofMeasure == null || string.IsNullOrEmpty(model.BaseUnitofMeasure.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الوحده الأساسيه للقياس" : "Missing Base Unit of Measure");
                    return new { responseStatus, GetList };
                }
                if (model.OrderTypeCode == null || string.IsNullOrEmpty(model.OrderTypeCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود نوع الطلب" : "Missing order type code");
                    return new { responseStatus, GetList };
                }

                if (!DBContext.OrderTypes.Any(x => x.OrderTypeCode == model.OrderTypeCode && x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود نوع الطلب " + model.OrderTypeCode + " خاطئ. أو لا يتعلق بالمصنع الخاص بأمر الشغل" : "Order type code " + model.OrderTypeCode + " is wrong. Or is not related to the process order plant code");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == model.ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.OrderTypes.Any(x => x.OrderTypeCode == model.OrderTypeCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود نوع الطلب خاطئ" : "Order type code is wrong");
                    return new { responseStatus, GetList };
                }
                var productData = DBContext.Products.FirstOrDefault(x => x.ProductCode == model.ProductCode);


                ProductionOrder entity = new()
                {
                    Uom = model.BaseUnitofMeasure,
                    PlantCode = model.PlantCode,
                    PlantCodePlanning = model.PlantCode,
                    ProductCode = model.ProductCode,
                    OrderTypeCode = model.OrderTypeCode,
                    IsMobile = true,
                    OrderDate = model.ProductionDate,
                    SapOrderId = model.SapOrderId,
                    Qty = model.QtyCartoon,
                    StartDate = DateTime.Now,
                    UserIdAdd = model.UserID,
                    DateTimeAdd = DateTime.Now,
                    IsCreatedOnSap = true,
                    IsCommingFromSap = true
                };
                DBContext.ProductionOrders.Add(entity);
                DBContext.SaveChanges();

                GetList = (from b in DBContext.ProductionOrders
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                           select new ProcessOrdersParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,

                               ProductionOrderId = b.ProductionOrderId,
                               SapOrderId = b.SapOrderId,
                               Qty = b.Qty,
                               OrderDate = b.OrderDate,
                               IsMobile = b.IsMobile,
                               IsCreatedOnSap = b.IsCreatedOnSap,
                               IsReleased = b.IsReleased
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        // This API for juhayna team to create purchase requisition here. (New 17062023)
        public object SapPurchaseRequisition_Insert([FromBody] SapPurchaseRequisitionListParam model)
        {
            ResponseStatus responseStatus = new();
            List<PurchaseRequisitionsParam> ErrorLog = new List<PurchaseRequisitionsParam>();

            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                foreach (var item in model.PurchaseRequisitions)
                {
                    if (item.PurchaseRequisitionNo == null || string.IsNullOrEmpty(item.PurchaseRequisitionNo.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رقم طلب الشراء" : "Missing purchase requisition no");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PurchaseRequisitionReleaseDate == null)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال تاريخ إصدار طلب الشراء" : "Missing purchase requisition release date");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PurchaseRequisitionQty == null || item.PurchaseRequisitionQty <= 0)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كمية طلب الشراء" : "Missing purchase requisition quantity");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PlantCode_Source == null || string.IsNullOrEmpty(item.PlantCode_Source.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع - المصدر" : "Missing plant code - source");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode_Source))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ - المصدر" : "Plant code is wrong - source");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PlantCode_Destination == null || string.IsNullOrEmpty(item.PlantCode_Destination.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع - الوجهة" : "Missing plant code - destination");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode_Destination))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ - الوجهة" : "Plant code is wrong - destination");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (item.StorageLocationCode_Source != null && !string.IsNullOrEmpty(item.StorageLocationCode_Source.Trim()))
                    {
                        if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.StorageLocationCode_Source && x.PlantCode == item.PlantCode_Source))
                        {
                            item.ErrorMsg = ((model.applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره - المصدر" : "Storage Location Code is wrong or not related to the selected plant - source");
                            ErrorLog.Add(item);
                            continue;
                        }
                    }

                    if (item.StorageLocationCode_Destination != null && !string.IsNullOrEmpty(item.StorageLocationCode_Destination.Trim()))
                    {
                        if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.StorageLocationCode_Destination && x.PlantCode == item.PlantCode_Destination))
                        {
                            item.ErrorMsg = ((model.applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره - الوجهة" : "Storage Location Code is wrong or not related to the selected plant - destination");
                            ErrorLog.Add(item);
                            continue;
                        }
                    }

                    if (item.LineNumber == null || item.LineNumber <= 0)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رقم السطر" : "Missing line number");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductCode == null || string.IsNullOrEmpty(item.ProductCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (!DBContext.Products.Any(x => x.ProductCode == item.ProductCode && x.PlantCode == item.PlantCode_Source))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المنتج خاطئ او لا ينتمي للمصنع التي تم اختياره - المصدر" : "Product code is wrong or not related to the selected plant - source");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ProductQty == null || item.ProductQty <= 0)
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كمية المنتج" : "Missing product quantity");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.Uom == null || string.IsNullOrEmpty(item.Uom.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال وحدة قياس" : "Missing unit of measurement");
                        ErrorLog.Add(item);
                        continue;
                    }

                    long PurchaseRequisitionId_Inserted = 0;

                    if (DBContext.PurchaseRequisitions.Any(x => x.PurchaseRequisitionNo == item.PurchaseRequisitionNo))
                    {
                        var entity = DBContext.PurchaseRequisitions.FirstOrDefault(x => x.PurchaseRequisitionNo == item.PurchaseRequisitionNo);

                        entity.PurchaseRequisitionQty = item.PurchaseRequisitionQty;
                        entity.PlantCodeSource = item.PlantCode_Source;
                        entity.StorageLocationCodeSource = item.StorageLocationCode_Source;
                        entity.PlantCodeDestination = item.PlantCode_Destination;
                        entity.StorageLocationCodeDestination = item.StorageLocationCode_Destination;
                        entity.PurchaseRequisitionReleaseDate = item.PurchaseRequisitionReleaseDate;

                        DBContext.PurchaseRequisitions.Update(entity);
                        DBContext.SaveChanges();
                        PurchaseRequisitionId_Inserted = entity.PurchaseRequisitionId;
                    }
                    else
                    {
                        PurchaseRequisition entity = new()
                        {
                            PurchaseRequisitionNo = item.PurchaseRequisitionNo,
                            PurchaseRequisitionQty = item.PurchaseRequisitionQty,
                            PlantCodeSource = item.PlantCode_Source,
                            StorageLocationCodeSource = item.StorageLocationCode_Source,
                            PlantCodeDestination = item.PlantCode_Destination,
                            StorageLocationCodeDestination = item.StorageLocationCode_Destination,
                            PurchaseRequisitionReleaseDate = item.PurchaseRequisitionReleaseDate,
                            PurchaseRequisitionStatus = "New",
                            UserIdAdd = 2
                        };
                        DBContext.PurchaseRequisitions.Add(entity);
                        DBContext.SaveChanges();
                        PurchaseRequisitionId_Inserted = entity.PurchaseRequisitionId;
                    }



                    if (DBContext.PurchaseRequisitionDetails.Any(x => x.PurchaseRequisitionNo == item.PurchaseRequisitionNo && x.ProductCode == item.ProductCode && x.LineNumber == item.LineNumber))
                    {
                        var entity = DBContext.PurchaseRequisitionDetails.FirstOrDefault(x => x.PurchaseRequisitionNo == item.PurchaseRequisitionNo && x.ProductCode == item.ProductCode);

                        entity.Qty = item.ProductQty;
                        entity.Uom = item.Uom;
                        entity.LineNumber = item.LineNumber;

                        DBContext.PurchaseRequisitionDetails.Update(entity);
                        DBContext.SaveChanges();
                    }
                    else
                    {
                        if (DBContext.PurchaseRequisitionDetails.Any(x => x.PurchaseRequisitionNo == item.PurchaseRequisitionNo && x.LineNumber == item.LineNumber && x.ProductCode != item.ProductCode))
                        {
                            item.ErrorMsg = ((model.applang == "ar") ? "رقم السطر هذا موجود بالفعل لكود منتج آخر لأمر الشراء المحدد" : "This line number already exists for another product code for the given purchase requisition");
                            ErrorLog.Add(item);
                            continue;
                        }

                        PurchaseRequisitionDetail entity = new()
                        {
                            PurchaseRequisitionNo = item.PurchaseRequisitionNo,
                            PurchaseRequisitionId = PurchaseRequisitionId_Inserted,
                            LineNumber = item.LineNumber,
                            LineStatus = "New",
                            ProductCode = item.ProductCode,
                            Uom = item.Uom,
                            UserIdAdd = 2,
                            Qty = item.ProductQty
                        };
                        DBContext.PurchaseRequisitionDetails.Add(entity);
                        DBContext.SaveChanges();
                    }
                }

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        // This API for juhayna team to create shipment here. (New 17062023)
        public object SapShipment_Insert([FromBody] SapShipmentAddListParam model)
        {
            ResponseStatus responseStatus = new();
            List<ShipmentsParam> ErrorLog = new List<ShipmentsParam>();
            try
            {
                if (string.IsNullOrEmpty(model.token) || model.token == null)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                var UserID = DecryptToken(model.token);

                if (UserID == -3 || UserID == -2 || UserID == -1)
                {
                    responseStatus.StatusCode = 403;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Please reactivate the session";
                    return new { responseStatus, ErrorLog };
                }
                foreach (var item in model.Shipments)
                {
                    if (item.ShipmentNo == null || string.IsNullOrEmpty(item.ShipmentNo.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال رقم الشحنه" : "Missing shipment no");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.ShipmentTypeCode == null || string.IsNullOrEmpty(item.ShipmentTypeCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود نوع الشحنه" : "Missing shipment type code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.RouteCode == null || string.IsNullOrEmpty(item.RouteCode.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing route code");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (item.PlantCode_Destination == null || string.IsNullOrEmpty(item.PlantCode_Destination.Trim()))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode_Destination))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (!DBContext.Routes.Any(x => x.RouteCode == item.RouteCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المسار خاطئ" : "Route code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.ShipmentTypePlants.Any(x => x.ShipmentTypeCode == item.ShipmentTypeCode))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود المسار خاطئ" : "Route code is wrong");
                        ErrorLog.Add(item);
                        continue;
                    }
                    if (!DBContext.ShipmentTypePlants.Any(x => x.ShipmentTypeCode == item.ShipmentTypeCode && x.PlantCode == item.PlantCode_Destination))
                    {
                        item.ErrorMsg = ((model.applang == "ar") ? "كود نوع الشحنه خاطئ او لا ينتمي للمصنع التي تم اختياره " : "Shipment type Code is wrong or not related to the selected plant");
                        ErrorLog.Add(item);
                        continue;
                    }

                    if (DBContext.Shipments.Any(x => x.ShipmentNo == item.ShipmentNo))
                    {
                        var entity = DBContext.Shipments.FirstOrDefault(x => x.ShipmentNo == item.ShipmentNo);

                        entity.RouteCode = item.RouteCode;
                        entity.PlantCodeDestination = item.PlantCode_Destination;
                        entity.ShipmentTypeCode = item.ShipmentTypeCode;

                        DBContext.Shipments.Update(entity);
                        DBContext.SaveChanges();
                    }
                    else
                    {
                        Shipment entity = new()
                        {
                            ShipmentNo = item.ShipmentNo,
                            SapshipmentId = 1,
                            RouteCode = item.RouteCode,
                            PlantCodeDestination = item.PlantCode_Destination,
                            ShipmentTypeCode = item.ShipmentTypeCode
                        };
                        DBContext.Shipments.Add(entity);
                        DBContext.SaveChanges();
                    }
                }

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, ErrorLog };
        }
        #endregion
        #region Operations
        [Route("[action]")]
        [HttpGet]
        public object GetProcessOrdersList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ProcessOrdersParam> GetList = new();
            List<ProcessOrderDetails> processOrderDetails = new();
            try
            {
                var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(pod=>pod.ProductionOrderId).Distinct().ToList();

                GetList = (from b in DBContext.ProductionOrders
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                           join u in DBContext.Users on b.UserIdAdd equals u.UserId
                           where b.IsClosed == false
                           select new ProcessOrdersParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               UserIdCreated = u.UserId,
                               UserNameCreated = u.UserName,
                               ProductionOrderId = b.ProductionOrderId,
                               SapOrderId = b.SapOrderId,
                               Qty = b.Qty,
                               QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               OrderDate = b.OrderDate,
                               IsMobile = b.IsMobile,
                               IsCreatedOnSap = b.IsCreatedOnSap,
                               IsReleased = b.IsReleased,
                               HasDetails = GetProductionOrderDetails.Contains(b.ProductionOrderId)
                           }).Distinct().ToList();
                        
                processOrderDetails = (from b in DBContext.ProductionOrderDetails
                                       join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                       join x in DBContext.ProductionLines on b.ProductionLineId equals x.ProductionLineId
                                       select new ProcessOrderDetails
                                       {
                                           ProductionOrderId = b.ProductionOrderId,
                                           BatchNo = b.BatchNo,
                                           ProductionLineId = x.ProductionLineId,
                                           OrderDetailsId = b.OrderDetailsId,
                                           ProductionDate = b.ProductionDate,
                                           ProductionLineCode = x.ProductionLineCode,
                                           Qty = b.Qty,
                                           QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                           IsClosedBatch = b.IsClosedBatch,
                                           IsReleased = b.IsReleased,
                                           BatchStatus = b.BatchStatus
                                       }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, processOrderDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProcessOrdersDetailsList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ProcessOrdersDetailsParam> GetList = new();
            List<ProcessOrderDetails> processOrderDetails = new();

            try
            {
                var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(x => x.ProductionOrderId).Distinct().ToList();
                processOrderDetails = (from b in DBContext.ProductionOrderDetails
                                       join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                       join l in DBContext.ProductionLines on b.ProductionLineId equals l.ProductionLineId
                                       select new ProcessOrderDetails
                                       {
                                           ProductionOrderId = b.ProductionOrderId,
                                           BatchNo = b.BatchNo,
                                           ProductionLineId = l.ProductionLineId,
                                           OrderDetailsId = b.OrderDetailsId,
                                           ProductionDate = b.ProductionDate.Value,
                                           ProductionLineCode = l.ProductionLineCode,
                                           Qty = b.Qty ?? 0,
                                           QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                           IsClosedBatch = b.IsClosedBatch,
                                           IsReleased = b.IsReleased ?? false,
                                           BatchStatus = b.BatchStatus
                                       }).Distinct().ToList();

                GetList = DBContext.VwProductionOrders.ToList().Select(x =>
                {
                    var BatchDetails = processOrderDetails.Where(c => c.ProductionOrderId == x.ProductionOrderId).ToList();
                    var Product = DBContext.Products.Where(c => c.ProductCode == x.ProductCode).FirstOrDefault();
                    return new ProcessOrdersDetailsParam
                    {
                        PlantId = x.PlantId,
                        PlantCode = x.PlantCode,
                        PlantDesc = x.PlantDesc,
                        OrderTypeId = x.OrderTypeId,
                        OrderTypeCode = x.OrderTypeCode,
                        OrderTypeDesc = x.OrderTypeDesc,
                        ProductId = x.ProductId,
                        ProductCode = x.ProductCode,
                        ProductDesc = x.ProductDesc,
                        ProductionOrderId = x.ProductionOrderId,
                        SapOrderId = x.SapOrderId,
                        Qty = x.Qty,
                        QtyCartoon = (x.Qty / (long)((Product.NumeratorforConversionPac ?? 0) / (Product.DenominatorforConversionPac ?? 0))),
                        OrderDate = x.OrderDate,
                        IsMobile = x.IsMobile,
                        IsCreatedOnSap = x.IsCreatedOnSap,
                        IsReleased = x.IsReleased,
                        HasDetails = GetProductionOrderDetails.Contains(x.ProductionOrderId),
                        processOrderDetails = BatchDetails
                    };
                }).Distinct().ToList();

                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProcessOrdersDetailsListForClose(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();

            List<ProcessOrderInDetails> GetList = new();
            List<ProductionOrderReceiving> productionOrderReceivings = new();

            try
            {
                productionOrderReceivings = (from b in DBContext.ProductionOrderReceivings select b).Distinct().ToList();

                List<ProcessOrderInDetails> GetBatchData = (from b in DBContext.ProductionOrderDetails
                                                            join l in DBContext.ProductionLines on b.ProductionLineId equals l.ProductionLineId
                                                            join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                                            where b.IsClosedBatch != true
                                                            select new ProcessOrderInDetails
                                                            {
                                                                ProductionOrderId = b.ProductionOrderId,
                                                                BatchNo = b.BatchNo,
                                                                ProductionLineId = l.ProductionLineId,
                                                                OrderDetailsId = b.OrderDetailsId,
                                                                ProductionDate = b.ProductionDate.Value,
                                                                ProductionLineCode = l.ProductionLineCode,
                                                                BatchQty = b.Qty ?? 0,
                                                                BatchQtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                                IsClosedBatch = b.IsClosedBatch,
                                                                IsReleased = b.IsReleased ?? false,
                                                                BatchStatus = b.BatchStatus,
                                                                ProductCode = p.ProductCode,
                                                                ProductDesc = ((applang == "ar") ? p.ProductDescAr : p.ProductDesc),
                                                                PalletCapacity = ((long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0))),

                                                            }).Distinct().ToList();
                GetList = GetBatchData.Select(b =>
                {
                    List<BatchDataParam> productionOrderReceiving = (from f in productionOrderReceivings
                                                                     where f.ProductionOrderId == b.ProductionOrderId && f.BatchNo == b.BatchNo
                                                                     select new BatchDataParam
                                                                     {
                                                                         PalletQty = f.Qty.Value,
                                                                         PalletCartoonQty = (f.Qty.Value / (long)((f.NumeratorforConversionPal ?? 0) / (f.DenominatorforConversionPal ?? 0))),
                                                                         PalletCode = f.PalletCode,
                                                                         NoCartoonPerPallet = (long)((f.NumeratorforConversionPal ?? 0) / (f.DenominatorforConversionPal ?? 0)),
                                                                         WarehouseReceivingQty = f.WarehouseReceivingQty.Value,
                                                                         WarehouseReceivingCartoonQty = (f.WarehouseReceivingQty.Value / (long)((f.NumeratorforConversionPac ?? 0) / (f.DenominatorforConversionPac ?? 0))),
                                                                     }).ToList();

                    return new ProcessOrderInDetails
                    {
                        ProductionOrderId = b.ProductionOrderId,
                        BatchNo = b.BatchNo,
                        ProductionLineId = b.ProductionLineId,
                        OrderDetailsId = b.OrderDetailsId,
                        ProductionDate = b.ProductionDate.Value,
                        ProductionLineCode = b.ProductionLineCode,
                        BatchQty = b.BatchQty ?? 0,
                        BatchQtyCartoon = b.BatchQtyCartoon ?? 0,
                        IsClosedBatch = b.IsClosedBatch ?? false,
                        IsReleased = b.IsReleased ?? false,
                        BatchStatus = b.BatchStatus,
                        ProductionOrderReceivingDetails = productionOrderReceiving,
                        ProductCode = b.ProductCode,
                        ProductDesc = b.ProductDesc,
                        PalletCapacity = b.PalletCapacity

                    };
                }).Distinct().ToList();

                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new
            {
                responseStatus,
                GetList
            };
        }
        [Route("[action]")]
        [HttpPost]
        public UpdateProcessOrderDto UpdateProcessOrderQuantity(UpdateProcessOrderBody body)
        {
            var response = new UpdateProcessOrderDto();
            try
            {
                if(body.processOrderId == null || body.processOrderId <= 0)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing process order id","لم يتم إدخال معرف أمر الإنتاج", body.appLang);
                    return response;
                }
                if (body.newQuantity == null || body.newQuantity <= 0)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing new quantity", "لم يتم إدخال الكمية الجديدة", body.appLang);
                    return response;
                }
                var productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == body.processOrderId);
                
                
                if (productionOrder == null)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Process order not found", "لم يتم العثور على أمر الإنتاج", body.appLang);
                    return response;
                }
                var product = DBContext.Products.FirstOrDefault(x => x.ProductCode == productionOrder.ProductCode);
                int noOfItemPerCarton = (int)(product.NumeratorforConversionPac / product.DenominatorforConversionPac);
                int orderCartonQty = (int)(productionOrder.Qty / noOfItemPerCarton);
                if (productionOrder.IsClosed == true)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Process order is already closed", "تم إغلاق أمر الإنتاج بالفعل", body.appLang);
                    return response;
                }
                if (orderCartonQty>= body.newQuantity)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("New quantity must be greater than the current quantity", "يجب أن تكون الكمية الجديدة أكبر من الكمية الحالية", body.appLang);
                    return response;
                }
                productionOrder.Qty = body.newQuantity * noOfItemPerCarton;
                DBContext.ProductionOrders.Update(productionOrder);
                DBContext.SaveChanges();
                response.responseStatus = ResponseStatusHelper.SuccessResponseStatus("Process order quantity updated successfully", "تم تحديث كمية أمر الإنتاج بنجاح", body.appLang);
                return response;
            }
            catch (Exception ex)
            {
                response.responseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.Message, body.appLang);
                Console.WriteLine(ex.InnerException);
            }
            return response;
        }
        [Route("[action]")]
        [HttpPost]
        public UpdateProcessOrderDto CloseProcessOrder(CloseProcessOrderBody body)
        {
            var response = new UpdateProcessOrderDto();
            try
            {
                if (body.processOrderId == null || body.processOrderId <= 0)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing process order id", "لم يتم إدخال معرف أمر الإنتاج", body.appLang);
                    return response;
                }
               
                var productionOrder = DBContext.ProductionOrders.FirstOrDefault(po => po.ProductionOrderId == body.processOrderId);
                if (productionOrder == null)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Process order not found", "لم يتم العثور على أمر الإنتاج", body.appLang);
                    return response;
                }
                if(productionOrder.IsClosed == true)
                {
                    response.responseStatus = ResponseStatusHelper.ErrorResponseStatus("Process order is already closed", "تم إغلاق أمر الإنتاج بالفعل", body.appLang);
                    return response;
                }

                productionOrder.IsClosed = true;
                DBContext.ProductionOrders.Update(productionOrder);
                DBContext.SaveChanges();
                response.responseStatus = ResponseStatusHelper.SuccessResponseStatus("Process order quantity closed successfully", "تم غلق كمية أمر الإنتاج بنجاح", body.appLang);
                return response;
            }
            catch (Exception ex)
            {
                response.responseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.Message, body.appLang);
            }
            return response;
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProcessOrderByID(long UserID, string DeviceSerialNo, string applang, long ProductionOrderId)
        {
            ResponseStatus responseStatus = new();
            ProcessOrdersParam processOrder = new();
            List<ProcessOrderDetails> processOrderDetails = new();
            try
            {
                var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(x => x.ProductionOrderId).Distinct().ToList();
                processOrder = (from b in DBContext.ProductionOrders
                                join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                                join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                                where b.ProductionOrderId == ProductionOrderId
                                select new ProcessOrdersParam
                                {
                                    PlantId = x.PlantId,
                                    PlantCode = x.PlantCode,
                                    PlantDesc = x.PlantDesc,
                                    OrderTypeId = o.OrderTypeId,
                                    OrderTypeCode = o.OrderTypeCode,
                                    OrderTypeDesc = o.OrderTypeDesc,
                                    ProductId = p.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductDesc = p.ProductDesc,

                                    ProductionOrderId = b.ProductionOrderId,
                                    SapOrderId = b.SapOrderId,
                                    Qty = b.Qty,
                                    QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                    OrderDate = b.OrderDate,
                                    IsMobile = b.IsMobile,
                                    IsCreatedOnSap = b.IsCreatedOnSap,
                                    IsReleased = b.IsReleased,
                                    HasDetails = GetProductionOrderDetails.Contains(b.ProductionOrderId)
                                }).FirstOrDefault();

                processOrderDetails = (from b in DBContext.ProductionOrderDetails
                                       join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                       join x in DBContext.ProductionLines on b.ProductionLineId equals x.ProductionLineId
                                       where b.ProductionOrderId == ProductionOrderId
                                       select new ProcessOrderDetails
                                       {
                                           ProductionOrderId = b.ProductionOrderId,
                                           BatchNo = b.BatchNo,
                                           ProductionLineId = x.ProductionLineId,
                                           OrderDetailsId = b.OrderDetailsId,
                                           ProductionDate = b.ProductionDate,
                                           ProductionLineCode = x.ProductionLineCode,
                                           Qty = b.Qty,
                                           QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                           IsClosedBatch = b.IsClosedBatch,
                                           IsReleased = b.IsReleased,
                                           BatchStatus = b.BatchStatus
                                       }).Distinct().ToList();
                if (processOrder != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, processOrder, processOrderDetails };
        }
        [Route("[action]")]
        [HttpPost]
        // This API is used by mobile to try to create a production order first in SAP and then create it here.
        public async Task<object> CreateProcessOrder([FromBody] CreateProcessOrderParam model)
        {
            ResponseStatus responseStatus = new();

            ProcessOrdersParam processOrder = new();
            try
            {
                if (model.QtyCartoon == null || model.QtyCartoon <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الكمية" : "Missing quantity");
                    return new { responseStatus, processOrder };
                }
                if (model.ProductionDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الطلب" : "Missing order date");
                    return new { responseStatus, processOrder };
                }
                if (model.BasicFinishDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الانتهاء الأساسي" : "Missing Basic Finish Date");
                    return new { responseStatus, processOrder };
                }
                if (model.ProductCode == null || string.IsNullOrEmpty(model.ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus, processOrder };
                }
                if (model.PlantCode == null || string.IsNullOrEmpty(model.PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, processOrder };
                }
                if (model.ProductionVersion == null || string.IsNullOrEmpty(model.ProductionVersion.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال نسخة الإنتاج" : "Missing Production Version");
                    return new { responseStatus, processOrder };
                }
                if (model.BaseUnitofMeasure == null || string.IsNullOrEmpty(model.BaseUnitofMeasure.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الوحده الأساسيه للقياس" : "Missing Base Unit of Measure");
                    return new { responseStatus, processOrder };
                }
                if (model.OrderTypeCode == null || string.IsNullOrEmpty(model.OrderTypeCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود نوع الطلب" : "Missing order type code");
                    return new { responseStatus, processOrder };
                }
                var OrderType = DBContext.OrderTypes.FirstOrDefault(x => x.OrderTypeCode == model.OrderTypeCode);
                Console.WriteLine("OrderType: " + OrderType.OrderTypeCode);
                Console.WriteLine("OrderTypePlantCode: " + OrderType.PlantCode);
                Console.WriteLine("PlantCode: " + model.PlantCode);
                Console.WriteLine("IsEqual: " + (OrderType.PlantCode == model.PlantCode));
                if (!DBContext.OrderTypes.Any(x => x.OrderTypeCode == model.OrderTypeCode && x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود نوع الطلب " + model.OrderTypeCode + " خاطئ. أو لا يتعلق بالمصنع الخاص بأمر الشغل" : "Order type code " + model.OrderTypeCode + " is wrong. Or is not related to the process order planet code");
                    
                    return new { responseStatus, processOrder };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == model.ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus, processOrder };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, processOrder };
                }
                if (!DBContext.OrderTypes.Any(x => x.OrderTypeCode == model.OrderTypeCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود نوع الطلب خاطئ" : "Order type code is wrong");
                    return new { responseStatus, processOrder };
                }
                var productData = DBContext.Products.FirstOrDefault(x => x.ProductCode == model.ProductCode);

                long SapOrderId = -1;
                bool IsCreatedOnSap = false;
                CreateSapOrderResponse Response = await SapService.CreateSapOrderAsync(
                    new CreateSapOrderParameters { PLNBEZ= model.ProductCode, WERKS= model.PlantCode, PWERK=model.PlantCode, AUART=model.OrderTypeCode, GAMNG=model.QtyCartoon.Value, GSTRP= model.BasicFinishDate.Value.ToString("dd.MM.yyyy"), GLTRP=model.BasicFinishDate.Value.ToString("dd.MM.yyyy"), GMEIN=model.BaseUnitofMeasure, VERID="" });

                //  CreateSapOrderResponse Response = SAPIntegrationAPI.CreateSapOrder(model.ProductCode, model.PlantCode, model.PlantCode, model.OrderTypeCode, model.QtyCartoon.Value, DateTime.Now.ToString("dd.mm.yyyy"), model.BasicFinishDate.Value.ToString("dd.mm.yyyy"), model.BaseUnitofMeasure, model.ProductionVersion);
                if (Response != null && Response.messageText == "Created")
                {
                    IsCreatedOnSap = true;
                    SapOrderId = long.Parse(Response.AUFNR);
                }
                else
                {
                    return new
                    {
                        responseStatus = new ResponseStatus
                        {
                            StatusCode = 500,
                            IsSuccess = false,
                            StatusMessage = ((model.applang == "ar") ? "خطأ من ساب" : "Error from sap") + "\n" + Response.messageText + " - " + Response.message,
                            ErrorMessage = Response.messageText + " - " + Response.message
                        },
                        processOrder
                    };
                }
                var Product = DBContext.Products.Where(c => c.ProductCode == model.ProductCode).FirstOrDefault();
                ProductionOrder entity = new()
                {
                    Uom = model.BaseUnitofMeasure,
                    PlantCode = model.PlantCode,
                    PlantCodePlanning = model.PlantCode,
                    ProductCode = model.ProductCode,
                    OrderTypeCode = model.OrderTypeCode,
                    IsMobile = true,
                    OrderDate = model.ProductionDate,
                    SapOrderId = SapOrderId,
                    Qty = (model.QtyCartoon * (long)((Product.NumeratorforConversionPac ?? 0) / (Product.DenominatorforConversionPac ?? 0))),
                    StartDate = DateTime.Now,
                    UserIdAdd = model.UserID,
                    DateTimeAdd = DateTime.Now,
                    IsCreatedOnSap = IsCreatedOnSap,
                    MessageCode = Response.messageCode,
                    MessageText = Response.messageText,
                    Message = Response.message,
                    IsCommingFromSap = false
                };
                DBContext.ProductionOrders.Add(entity);
                DBContext.SaveChanges();

                processOrder = (from b in DBContext.ProductionOrders
                                join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                                join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                                where b.ProductionOrderId == entity.ProductionOrderId
                                select new ProcessOrdersParam
                                {
                                    PlantId = x.PlantId,
                                    PlantCode = x.PlantCode,
                                    PlantDesc = x.PlantDesc,
                                    OrderTypeId = o.OrderTypeId,
                                    OrderTypeCode = o.OrderTypeCode,
                                    OrderTypeDesc = o.OrderTypeDesc,
                                    ProductId = p.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductDesc = p.ProductDesc,

                                    ProductionOrderId = b.ProductionOrderId,
                                    SapOrderId = b.SapOrderId,
                                    Qty = b.Qty,
                                    QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                    OrderDate = b.OrderDate,
                                    IsMobile = b.IsMobile,
                                    IsCreatedOnSap = b.IsCreatedOnSap,
                                    IsReleased = b.IsReleased,
                                    HasDetails = false
                                }).Distinct().FirstOrDefault();
                if (processOrder != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, processOrder };
        }
        [Route("[action]")]
        [HttpPost]
        public object AddProcessOrderDetails([FromBody] AddProcessOrderDetailsParam model)
        {
            ResponseStatus responseStatus = new();
            ProcessOrdersParam processOrder = new();
            List<ProcessOrderDetails> processOrderDetails = new();
            try
            {
                if (model.batchList == null || model.batchList.Count <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تفاصيل أمر الشغل" : "Missing order details");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
                if (productionOrder == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (productionOrder.IsReleased == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم تحرير أمر الشغل" : "Process Order is released");
                    return new { responseStatus, processOrder, processOrderDetails };
                }

                foreach (BatchDetails item in model.batchList)
                {
                    if (item.QtyCartoon == null || item.QtyCartoon <= 0)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الكمية" : "Missing quantity");
                        return new { responseStatus, processOrder, processOrderDetails };
                    }
                    if (item.ProductionDate == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الانتاج" : "Missing production date");
                        return new { responseStatus, processOrder, processOrderDetails };
                    }
                    if (item.ProductionLineCode == null || string.IsNullOrEmpty(item.ProductionLineCode.Trim()))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                        return new { responseStatus, processOrder, processOrderDetails };
                    }
                    if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == item.ProductionLineCode && x.PlantCode == productionOrder.PlantCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "كود خط الإنتاج " + item.ProductionLineCode + " خاطئ. أو لا يتعلق بالمصنع الخاص بأمر الشغل" : "Production line code " + item.ProductionLineCode + " is wrong. Or is not related to the process order planet code");
                        return new { responseStatus, processOrder, processOrderDetails };
                    }
                    if (item.BatchNo == null || string.IsNullOrEmpty(item.BatchNo.Trim()))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
                        return new { responseStatus, processOrder, processOrderDetails };
                    }
                    //if (item.BatchNo.Length != 10)
                    //{
                    //    responseStatus.StatusCode = 401;
                    //    responseStatus.IsSuccess = false;
                    //    responseStatus.StatusMessage = ((model.applang == "ar") ? "طول رقم الباتش " + item.BatchNo + " لا يساوي 10" : "Batch no " + item.BatchNo + " length is not equal to 10");
                    //    return new { responseStatus, processOrder, processOrderDetails };
                    //}
                    //if (DBContext.ProductionOrderDetails.Any(x => x.BatchNo == item.BatchNo && x.ProductionOrderId == productionOrder.ProductionOrderId))
                    //{
                    //    responseStatus.StatusCode = 401;
                    //    responseStatus.IsSuccess = false;
                    //    responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش  " + item.BatchNo + " موجوده مسبقا" : "Batch no " + item.BatchNo + " is already exist");
                    //    return new { responseStatus, processOrder, processOrderDetails };
                    //}
                }
                var productData = DBContext.Products.FirstOrDefault(x => x.ProductCode == productionOrder.ProductCode);

                foreach (BatchDetails item in model.batchList)
                {
                    var GetProductionLineData = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == item.ProductionLineCode);
                    ProductionOrderDetail productionOrderDetail = new()
                    {
                        BatchNo = item.BatchNo,
                        ProductionDate = item.ProductionDate,
                        ProductionOrderId = productionOrder.ProductionOrderId,
                        SapOrderId = productionOrder.SapOrderId ?? -1,
                        ProductionLineId = GetProductionLineData.ProductionLineId,
                        Qty = (item.QtyCartoon * (long)((productData.NumeratorforConversionPac ?? 0) / (productData.DenominatorforConversionPac ?? 0))),
                        PlantCode = productionOrder.PlantCode,
                        UserIdAdd = model.UserID,
                        DateTimeAdd = DateTime.Now,
                        BatchStatus = "Released",
                        ProductCode = productionOrder.ProductCode,
                        IsReleased = true,
                        DateTimeRelease = DateTime.Now,
                        UserIdRelease = model.UserID,
                        DeviceSerialNo = model.DeviceSerialNo,
                        DeviceSerialNoRelease = model.DeviceSerialNo

                    };
                    DBContext.ProductionOrderDetails.Add(productionOrderDetail);
                    DBContext.SaveChanges();
                    BatchList batchList = new()
                    {
                        BatchNo = item.BatchNo,
                        PlantCode = productionOrder.PlantCode,
                        ProductCode = productionOrder.ProductCode,
                        ProductionDate = item.ProductionDate,
                        ProductionLineId = GetProductionLineData.ProductionLineId,
                        QtyProduction = (item.QtyCartoon * (long)((productData.NumeratorforConversionPac ?? 0) / (productData.DenominatorforConversionPac ?? 0))),
                        SaporderId = productionOrder.SapOrderId ?? -1,
                        ProductionOrderDetailsId = productionOrderDetail.OrderDetailsId

                    };
                    DBContext.BatchLists.Add(batchList);
                    
                    ProductionLineWip productionLineWip = new()
                {
                    BatchNo = item.BatchNo,
                    DateTimeAdd = DateTime.Now,
                    DeviceSerialNo = model.DeviceSerialNo,
                    OrderDetailsId = productionOrderDetail.OrderDetailsId,
                    PlantCode = productionOrderDetail.PlantCode,
                    ProductionDate = productionOrderDetail.ProductionDate,
                    ProductionLineId = productionOrderDetail.ProductionLineId,
                    ProductionOrderId = productionOrderDetail.ProductionOrderId,
                    Qty = productionOrderDetail.Qty,
                    SapOrderId = productionOrderDetail.SapOrderId,
                    UserIdAdd = model.UserID

                };
                DBContext.ProductionLineWips.Add(productionLineWip);

                }
                DBContext.SaveChanges();
                

                var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(x => x.ProductionOrderId).Distinct().ToList();

                processOrder = (from b in DBContext.ProductionOrders
                                join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                                join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                                join u in DBContext.Users on b.UserIdAdd equals u.UserId
                                where b.ProductionOrderId == productionOrder.ProductionOrderId
                                select new ProcessOrdersParam
                                {
                                    PlantId = x.PlantId,
                                    PlantCode = x.PlantCode,
                                    PlantDesc = x.PlantDesc,
                                    OrderTypeId = o.OrderTypeId,
                                    OrderTypeCode = o.OrderTypeCode,
                                    OrderTypeDesc = o.OrderTypeDesc,
                                    ProductId = p.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductDesc = p.ProductDesc,
                                    UserIdCreated = u.UserId,
                                    UserNameCreated = u.UserName,
                                    ProductionOrderId = b.ProductionOrderId,
                                    SapOrderId = b.SapOrderId,
                                    Qty = b.Qty,
                                    QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                    OrderDate = b.OrderDate,
                                    IsMobile = b.IsMobile,
                                    IsCreatedOnSap = b.IsCreatedOnSap,
                                    IsReleased = b.IsReleased,
                                    HasDetails = GetProductionOrderDetails.Contains(b.ProductionOrderId)
                                }).FirstOrDefault();

                processOrderDetails = (from b in DBContext.ProductionOrderDetails
                                       join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                       join x in DBContext.ProductionLines on b.ProductionLineId equals x.ProductionLineId
                                       where b.ProductionOrderId == productionOrder.ProductionOrderId
                                       select new ProcessOrderDetails
                                       {
                                           ProductionOrderId = b.ProductionOrderId,
                                           BatchNo = b.BatchNo,
                                           ProductionLineId = x.ProductionLineId,
                                           OrderDetailsId = b.OrderDetailsId,
                                           ProductionDate = b.ProductionDate,
                                           ProductionLineCode = x.ProductionLineCode,
                                           Qty = b.Qty,
                                           QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                           IsClosedBatch = b.IsClosedBatch,
                                           IsReleased = b.IsReleased,
                                           BatchStatus = b.BatchStatus
                                       }).Distinct().ToList();
                if (processOrder != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, processOrder, processOrderDetails };
        }


        [Route("[action]")]
        [HttpPost]

        // This API is now cancelled 
        public object ReleaseBatch([FromBody] ReleaseBatchParam model)
        {
            ResponseStatus responseStatus = new();
            ProcessOrdersParam processOrder = new();
            List<ProcessOrderDetails> processOrderDetails = new();
            try
            {
                ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
                if (productionOrder == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (model.BatchNo == null || string.IsNullOrEmpty(model.BatchNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (model.ProductionLineCode == null || string.IsNullOrEmpty(model.ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (model.OrderDetailsId == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم معرف امر الشغل" : "Missing OrderDetailsId");
                    return new { responseStatus };
                }
                if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == model.ProductionLineCode && x.PlantCode == productionOrder.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود خط الإنتاج " + model.ProductionLineCode + " خاطئ. أو لا يتعلق بالمصنع الخاص بأمر الشغل" : "Production line code " + model.ProductionLineCode + " is wrong. Or is not related to the process order planet code");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                var GetProductionLineData = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == model.ProductionLineCode);

                ProductionOrderDetail productionOrderDetail = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId && x.BatchNo == model.BatchNo && x.ProductionLineId == GetProductionLineData.ProductionLineId && x.OrderDetailsId == model.OrderDetailsId);
                if (productionOrderDetail == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش لا ينتمي لأمر الشغل التي تم اختياره" : "Batch no is not related to the specified production Order");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (productionOrderDetail.IsReleased == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم تحرير رقم الباتش من قبل" : "Batch no is already released");
                    return new { responseStatus, processOrder, processOrderDetails };
                }
                if (DBContext.ProductionLineWips.Any(x => x.ProductionLineId == productionOrderDetail.ProductionLineId))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "خط الإنتاج مستخدم بالفعل" : "Production line is already used");
                    return new { responseStatus, processOrder, processOrderDetails };
                }

                productionOrderDetail.IsReleased = true;
                productionOrderDetail.DateTimeRelease = DateTime.Now;
                productionOrderDetail.UserIdRelease = model.UserID;
                productionOrderDetail.DeviceSerialNoRelease = model.DeviceSerialNo;
                productionOrderDetail.BatchStatus = "Released";
                DBContext.ProductionOrderDetails.Update(productionOrderDetail);
                DBContext.SaveChanges();

                ProductionLineWip productionLineWip = new()
                {
                    BatchNo = productionOrderDetail.BatchNo,
                    DateTimeAdd = DateTime.Now,
                    DeviceSerialNo = model.DeviceSerialNo,
                    OrderDetailsId = productionOrderDetail.OrderDetailsId,
                    PlantCode = productionOrderDetail.PlantCode,
                    ProductionDate = productionOrderDetail.ProductionDate,
                    ProductionLineId = productionOrderDetail.ProductionLineId,
                    ProductionOrderId = productionOrderDetail.ProductionOrderId,
                    Qty = productionOrderDetail.Qty,
                    SapOrderId = productionOrderDetail.SapOrderId,
                    UserIdAdd = model.UserID

                };
                DBContext.ProductionLineWips.Add(productionLineWip);
                DBContext.SaveChanges();

                var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(x => x.ProductionOrderId).Distinct().ToList();

                processOrder = (from b in DBContext.ProductionOrders
                                join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                                join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                                where b.ProductionOrderId == productionOrder.ProductionOrderId
                                select new ProcessOrdersParam
                                {
                                    PlantId = x.PlantId,
                                    PlantCode = x.PlantCode,
                                    PlantDesc = x.PlantDesc,
                                    OrderTypeId = o.OrderTypeId,
                                    OrderTypeCode = o.OrderTypeCode,
                                    OrderTypeDesc = o.OrderTypeDesc,
                                    ProductId = p.ProductId,
                                    ProductCode = p.ProductCode,
                                    ProductDesc = p.ProductDesc,

                                    ProductionOrderId = b.ProductionOrderId,
                                    SapOrderId = b.SapOrderId,
                                    Qty = b.Qty,
                                    QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                    OrderDate = b.OrderDate,
                                    IsMobile = b.IsMobile,
                                    IsCreatedOnSap = b.IsCreatedOnSap,
                                    IsReleased = b.IsReleased,
                                    HasDetails = GetProductionOrderDetails.Contains(b.ProductionOrderId)
                                }).FirstOrDefault();

                processOrderDetails = (from b in DBContext.ProductionOrderDetails
                                       join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                       join x in DBContext.ProductionLines on b.ProductionLineId equals x.ProductionLineId
                                       where b.ProductionOrderId == productionOrder.ProductionOrderId
                                       select new ProcessOrderDetails
                                       {
                                           ProductionOrderId = b.ProductionOrderId,
                                           BatchNo = b.BatchNo,
                                           ProductionLineId = x.ProductionLineId,
                                           OrderDetailsId = b.OrderDetailsId,
                                           ProductionDate = b.ProductionDate,
                                           ProductionLineCode = x.ProductionLineCode,
                                           Qty = b.Qty,
                                           QtyCartoon = (b.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                           IsClosedBatch = b.IsClosedBatch,
                                           IsReleased = b.IsReleased,
                                           BatchStatus = b.BatchStatus

                                       }).Distinct().ToList();
                if (processOrder != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "تم تحرير رقم الباتش بنجاح" : "Batch released successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, processOrder, processOrderDetails };
        }

        //public object ReleaseBatch([FromBody] ReleaseBatchParam model)
        //{
        //    ResponseStatus responseStatus = new();
        //    ProcessOrdersParam processOrder = new();
        //    List<ProcessOrderDetails> processOrderDetails = new();
        //    try
        //    {
        //        ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
        //        if (productionOrder == null)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
        //            return new { responseStatus, processOrder, processOrderDetails };
        //        }
        //        if (model.BatchNo == null || string.IsNullOrEmpty(model.BatchNo.Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
        //            return new { responseStatus, processOrder, processOrderDetails };
        //        }
        //        ProductionOrderDetail productionOrderDetail = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId && x.BatchNo == model.BatchNo);
        //        if (productionOrderDetail == null)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش لا ينتمي لأمر الشغل التي تم اختياره" : "Batch no is not related to the specified production Order");
        //            return new { responseStatus, processOrder, processOrderDetails };
        //        }
        //        if (productionOrderDetail.IsReleased == true)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "تم تحرير رقم الباتش من قبل" : "Batch no is already released");
        //            return new { responseStatus, processOrder, processOrderDetails };
        //        }
        //        if (DBContext.ProductionLineWips.Any(x => x.ProductionLineId == productionOrderDetail.ProductionLineId))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "خط الإنتاج مستخدم بالفعل" : "Production line is already used");
        //            return new { responseStatus, processOrder, processOrderDetails };
        //        }

        //        productionOrderDetail.IsReleased = true;
        //        productionOrderDetail.DateTimeRelease = DateTime.Now;
        //        productionOrderDetail.UserIdRelease = model.UserID;
        //        productionOrderDetail.DeviceSerialNoRelease = model.DeviceSerialNo;
        //        productionOrderDetail.BatchStatus = "Released";
        //        DBContext.ProductionOrderDetails.Update(productionOrderDetail);
        //        DBContext.SaveChanges();

        //        ProductionLineWip productionLineWip = new()
        //        {
        //            BatchNo = productionOrderDetail.BatchNo,
        //            DateTimeAdd = DateTime.Now,
        //            DeviceSerialNo = model.DeviceSerialNo,
        //            OrderDetailsId = productionOrderDetail.OrderDetailsId,
        //            PlantCode = productionOrderDetail.PlantCode,
        //            ProductionDate = productionOrderDetail.ProductionDate,
        //            ProductionLineId = productionOrderDetail.ProductionLineId,
        //            ProductionOrderId = productionOrderDetail.ProductionOrderId,
        //            Qty = productionOrderDetail.Qty,
        //            SapOrderId = productionOrderDetail.SapOrderId,
        //            UserIdAdd = model.UserID

        //        };
        //        DBContext.ProductionLineWips.Update(productionLineWip);
        //        DBContext.SaveChanges();

        //        var GetProductionOrderDetails = DBContext.ProductionOrderDetails.Select(x => x.ProductionOrderId).Distinct().ToList();

        //        processOrder = (from b in DBContext.ProductionOrders
        //                        join p in DBContext.Products on b.ProductCode equals p.ProductCode
        //                        join x in DBContext.Plants on b.PlantCode equals x.PlantCode
        //                        join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
        //                        where b.ProductionOrderId == productionOrder.ProductionOrderId
        //                        select new ProcessOrdersParam
        //                        {
        //                            PlantId = x.PlantId,
        //                            PlantCode = x.PlantCode,
        //                            PlantDesc = x.PlantDesc,
        //                            OrderTypeId = o.OrderTypeId,
        //                            OrderTypeCode = o.OrderTypeCode,
        //                            OrderTypeDesc = o.OrderTypeDesc,
        //                            ProductId = p.ProductId,
        //                            ProductCode = p.ProductCode,
        //                            ProductDesc = p.ProductDesc,

        //                            ProductionOrderId = b.ProductionOrderId,
        //                            SapOrderId = b.SapOrderId,
        //                            Qty = b.Qty,
        //                            OrderDate = b.OrderDate,
        //                            IsMobile = b.IsMobile,
        //                            IsCreatedOnSap = b.IsCreatedOnSap,
        //                            IsReleased = b.IsReleased,
        //                            HasDetails = GetProductionOrderDetails.Contains(b.ProductionOrderId)
        //                        }).FirstOrDefault();

        //        processOrderDetails = (from b in DBContext.ProductionOrderDetails
        //                               join x in DBContext.ProductionLines on b.ProductionLineId equals x.ProductionLineId
        //                               where b.ProductionOrderId == productionOrder.ProductionOrderId
        //                               select new ProcessOrderDetails
        //                               {
        //                                   ProductionOrderId = b.ProductionOrderId,
        //                                   BatchNo = b.BatchNo,
        //                                   ProductionLineId = x.ProductionLineId,
        //                                   OrderDetailsId = b.OrderDetailsId,
        //                                   ProductionDate = b.ProductionDate,
        //                                   ProductionLineCode = x.ProductionLineCode,
        //                                   Qty = b.Qty,
        //                                   IsClosedBatch = b.IsClosedBatch,
        //                                   IsReleased = b.IsReleased,
        //                                   BatchStatus = b.BatchStatus

        //                               }).Distinct().ToList();
        //        if (processOrder != null)
        //        {
        //            responseStatus.StatusCode = 200;
        //            responseStatus.IsSuccess = true;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "تم تحرير رقم الباتش بنجاح" : "Batch released successfully");
        //        }
        //        else
        //        {
        //            responseStatus.StatusCode = 400;
        //            responseStatus.IsSuccess = false;

        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "لا توجد بيانات" : "No data found");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseStatus.StatusCode = 500;
        //        responseStatus.IsSuccess = false;
        //        responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

        //        responseStatus.ErrorMessage = ex.Message;
        //    }
        //    return new { responseStatus, processOrder, processOrderDetails };
        //}
        [Route("[action]")]
        [HttpPost]
        public object CloseProductionBatch([FromBody] ReleaseBatchParam model)
        {
            ResponseStatus responseStatus = new();
            try
            {
                ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
                if (productionOrder == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
                    return new { responseStatus };
                }
                if (model.BatchNo == null || string.IsNullOrEmpty(model.BatchNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
                    return new { responseStatus };
                }
                if (model.OrderDetailsId == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم معرف امر الشغل" : "Missing OrderDetailsId");
                    return new { responseStatus };
                }
                List<ProductionOrderDetail> productionOrderDetail = DBContext.ProductionOrderDetails.Where(x => x.ProductionOrderId == model.ProductionOrderId && x.BatchNo == model.BatchNo && x.OrderDetailsId == model.OrderDetailsId).ToList();
                if (productionOrderDetail == null || productionOrderDetail.Count <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش لا ينتمي لأمر الشغل التي تم اختياره" : "Batch no is not related to the specified production Order");
                    return new { responseStatus };
                }
                foreach (var item in productionOrderDetail)
                {
                    if (item.IsReleased != true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم تحرير رقم الباتش من قبل" : "Batch no is not released yet");
                        return new { responseStatus };
                    }
                    if (item.IsClosedBatch == true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "تم غلق رقم الباتش من قبل" : "Batch no is already Closed");
                        return new { responseStatus };
                    }

                    item.IsClosedBatch = true;
                    item.BatchStatus = "Closed";
                    DBContext.ProductionOrderDetails.Update(item);
                    DBContext.SaveChanges();
                    productionOrder.IsClosed = !DBContext.ProductionOrderDetails
                        .Any(x => x.ProductionOrderId == productionOrder.ProductionOrderId && x.IsClosedBatch == false);
                    DBContext.ProductionOrders.Update(productionOrder);
                    var GetProductionLineWip = DBContext.ProductionLineWips.Where(x => x.ProductionLineId == item.ProductionLineId && x.ProductionOrderId == item.ProductionOrderId && x.BatchNo == item.BatchNo).FirstOrDefault();
                    DBContext.ProductionLineWips.Remove(GetProductionLineWip);
                    DBContext.SaveChanges();
                }


                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;

                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        //public object CloseProductionBatch([FromBody] ReleaseBatchParam model)
        //{
        //    ResponseStatus responseStatus = new();
        //    try
        //    {
        //        ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
        //        if (productionOrder == null)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
        //            return new { responseStatus };
        //        }
        //        if (model.BatchNo == null || string.IsNullOrEmpty(model.BatchNo.Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
        //            return new { responseStatus };
        //        }
        //        ProductionOrderDetail productionOrderDetail = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId && x.BatchNo == model.BatchNo);
        //        if (productionOrderDetail == null)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش لا ينتمي لأمر الشغل التي تم اختياره" : "Batch no is not related to the specified production Order");
        //            return new { responseStatus };
        //        }
        //        if (productionOrderDetail.IsReleased != true)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم تحرير رقم الباتش من قبل" : "Batch no is not released yet");
        //            return new { responseStatus };
        //        }
        //        if (productionOrderDetail.IsClosedBatch == true)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((model.applang == "ar") ? "تم غلق رقم الباتش من قبل" : "Batch no is already Closed");
        //            return new { responseStatus };
        //        }

        //        productionOrderDetail.IsClosedBatch = true;
        //        productionOrderDetail.BatchStatus = "Closed";
        //        DBContext.ProductionOrderDetails.Update(productionOrderDetail);
        //        DBContext.SaveChanges();

        //        var GetProductionLineWip = DBContext.ProductionLineWips.Where(x => x.ProductionLineId == productionOrderDetail.ProductionLineId && x.ProductionOrderId == productionOrderDetail.ProductionOrderId && x.BatchNo == productionOrderDetail.BatchNo).FirstOrDefault();
        //        DBContext.ProductionLineWips.Remove(GetProductionLineWip);
        //        DBContext.SaveChanges();

        //        responseStatus.StatusCode = 200;
        //        responseStatus.IsSuccess = true;

        //        responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

        //    }
        //    catch (Exception ex)
        //    {
        //        responseStatus.StatusCode = 500;
        //        responseStatus.IsSuccess = false;
        //        responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

        //        responseStatus.ErrorMessage = ex.Message;
        //    }
        //    return new { responseStatus };
        //}

        [Route("[action]")]
        [HttpGet]
        public object GetRunningBatches(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            try
            {
                var productionLineDetails = (from d in DBContext.ProductionOrderDetails
                                             join l in DBContext.ProductionOrders on d.ProductionOrderId equals l.ProductionOrderId
                                             join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                             join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                             join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                             join ln in DBContext.ProductionLines on d.ProductionLineId equals ln.ProductionLineId
                                             where d.IsReleased == true && d.IsClosedBatch != true && l.IsClosed != true
                                             select new
                                             {
                                                 PlantId = x.PlantId,
                                                 PlantCode = x.PlantCode,
                                                 PlantDesc = x.PlantDesc,
                                                 OrderTypeId = o.OrderTypeId,
                                                 OrderTypeCode = o.OrderTypeCode,
                                                 OrderTypeDesc = o.OrderTypeDesc,
                                                 ProductId = p.ProductId,
                                                 ProductCode = p.ProductCode,
                                                 ProductDesc = (applang == "ar")?p.ProductDescAr:p.ProductDesc,
                                                 ProductionOrderId = d.ProductionOrderId,
                                                 SapOrderId = d.SapOrderId,
                                                 ProductionOrderQty = l.Qty,
                                                 ProductionOrderQtyCartoon = (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                 OrderDate = l.OrderDate,
                                                 OrderDetailsId = d.OrderDetailsId,
                                                 BatchNo = d.BatchNo,
                                                 BatchQty = d.Qty,
                                                 BatchQtyCartoon = (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                 ProductionDate = d.ProductionDate,
                                                 IsClosedBatch = d.IsClosedBatch,
                                                 IsReleased = d.IsReleased,
                                                 BatchStatus = d.BatchStatus,
                                                 IsReceived = d.IsReceived,
                                                 ReceivedQty = d.ReceivedQty,
                                                 ReceivedQtyCartoon = (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                 ProductionLineId = ln.ProductionLineId,
                                                 ProductionLineCode = ln.ProductionLineCode,
                                                 ProductionLineDesc = ln.ProductionLineDesc,
                                                 noCartoonPerPallet = (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0))

                                             }).Distinct().ToList();



                if (productionLineDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                    return new { responseStatus, productionLineDetails };
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }

        [Route("[action]")]
        [HttpGet]
        public object GetProductionLineBatchs(long UserID, string DeviceSerialNo, string PlantCode, string ProductionLineCode, string applang)
        {
            ResponseStatus responseStatus = new();
            try
            {
                var batchList = (from d in DBContext.ProductionOrderDetails
                                 join l in DBContext.ProductionOrders on d.ProductionOrderId equals l.ProductionOrderId
                                 join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join ln in DBContext.ProductionLines on d.ProductionLineId equals ln.ProductionLineId
                                 where d.IsReleased == true && d.IsClosedBatch != true && d.PlantCode == PlantCode && ln.ProductionLineCode == ProductionLineCode
                                 select new
                                 {
                                     BatchID = d.BatchNo,
                                     BatchName = d.BatchNo
                                 }).Distinct().ToList();

                if (batchList != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                    return new { responseStatus, batchList };
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProductionLineDetails(long UserID, string DeviceSerialNo, string applang, string PlantCode, string ProductionLineCode)
        {
            ResponseStatus responseStatus = new();
            ProductionLineDetailsParam productionLineDetails = new();
            try
            {
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, productionLineDetails };
                }
                if (ProductionLineCode == null || string.IsNullOrEmpty(ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == ProductionLineCode && x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود خط الإنتاج لا ينتمي للمصنع" : "Production Line code is not related to the plant");
                    return new { responseStatus, productionLineDetails };
                }

                var GetProductionLineDetails = (from b in DBContext.ProductionLineWips
                                                join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                                join r in DBContext.ProductionOrderReceivings on new { b.ProductionOrderId, b.BatchNo } equals new { ProductionOrderId = r.ProductionOrderId.Value, r.BatchNo } into rr
                                                from r in rr.DefaultIfEmpty()
                                                join d in DBContext.ProductionOrderDetails on b.OrderDetailsId equals d.OrderDetailsId
                                                join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                                join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                                join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                                join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                                where ln.ProductionLineCode == ProductionLineCode && ln.PlantCode == PlantCode
                                                select new ProductionLineDetailsParam
                                                {
                                                    PlantId = x.PlantId,
                                                    PlantCode = x.PlantCode,
                                                    PlantDesc = x.PlantDesc,
                                                    OrderTypeId = o.OrderTypeId,
                                                    OrderTypeCode = o.OrderTypeCode,
                                                    OrderTypeDesc = o.OrderTypeDesc,
                                                    ProductId = p.ProductId,
                                                    ProductCode = p.ProductCode,
                                                    ProductDesc = (applang == "ar") ? p.ProductDescAr : p.ProductDesc,
                                                    ProductionOrderId = b.ProductionOrderId,
                                                    SapOrderId = b.SapOrderId,
                                                    ProductionOrderQty = l.Qty,
                                                    ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    OrderDate = l.OrderDate,
                                                    OrderDetailsId = b.OrderDetailsId,
                                                    BatchNo = d.BatchNo,
                                                    BatchQty = d.Qty,
                                                    BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    NoCanPerCartoon = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0)) : (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                                                    NoCartoonPerPallet = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) : (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0)),
                                                    ProductionDate = d.ProductionDate,
                                                    IsClosedBatch = d.IsClosedBatch,
                                                    IsReleased = d.IsReleased,
                                                    BatchStatus = d.BatchStatus,
                                                    IsReceived = d.IsReceived,
                                                    ReceivedQty = d.ReceivedQty,
                                                    ReceivedQtyCartoon = (r.BatchNo != null) ? (d.ReceivedQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    ProductionLineId = ln.ProductionLineId,
                                                    ProductionLineCode = ln.ProductionLineCode,
                                                    ProductionLineDesc = ln.ProductionLineDesc
                                                }).Distinct().ToList();

                productionLineDetails = (from b in GetProductionLineDetails
                                         select new ProductionLineDetailsParam
                                         {
                                             PlantId = b.PlantId,
                                             PlantCode = b.PlantCode,
                                             PlantDesc = b.PlantDesc,
                                             OrderTypeId = b.OrderTypeId,
                                             OrderTypeCode = b.OrderTypeCode,
                                             OrderTypeDesc = b.OrderTypeDesc,
                                             ProductId = b.ProductId,
                                             ProductCode = b.ProductCode,
                                             ProductDesc = b.ProductDesc,
                                             ProductionOrderId = b.ProductionOrderId,
                                             SapOrderId = b.SapOrderId,
                                             ProductionOrderQty = b.ProductionOrderQty,
                                             ProductionOrderQtyCartoon = b.ProductionOrderQtyCartoon,
                                             OrderDate = b.OrderDate,
                                             OrderDetailsId = b.OrderDetailsId,
                                             BatchNo = b.BatchNo,
                                             BatchQty = b.BatchQty,
                                             BatchQtyCartoon = b.BatchQtyCartoon,
                                             NoCanPerCartoon = b.NoCanPerCartoon,
                                             NoCartoonPerPallet = b.NoCartoonPerPallet,
                                             PalletCapacity = (b.NoCartoonPerPallet * b.NoCanPerCartoon),
                                             ProductionDate = b.ProductionDate,
                                             IsClosedBatch = b.IsClosedBatch,
                                             IsReleased = b.IsReleased,
                                             BatchStatus = b.BatchStatus,
                                             IsReceived = b.IsReceived,
                                             ReceivedQty = b.ReceivedQty,
                                             ReceivedQtyCartoon = b.ReceivedQtyCartoon,
                                             ProductionLineId = b.ProductionLineId,
                                             ProductionLineCode = b.ProductionLineCode,
                                             ProductionLineDesc = b.ProductionLineDesc
                                         }).FirstOrDefault();

                if (productionLineDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, productionLineDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProductionLineDetails2(long UserID, string DeviceSerialNo, string applang, string PlantCode, string ProductionLineCode)
        {
            ResponseStatus responseStatus = new();
            List<ProductionLineDetailsParam> productionLineDetails = new();
            try
            {
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, productionLineDetails };
                }
                if (ProductionLineCode == null || string.IsNullOrEmpty(ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == ProductionLineCode && x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود خط الإنتاج لا ينتمي للمصنع" : "Production Line code is not related to the plant");
                    return new { responseStatus, productionLineDetails };
                }

                var GetProductionLineDetails = (from b in DBContext.ProductionLineWips
                                                join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                                join r in DBContext.ProductionOrderReceivings on new { b.ProductionOrderId, b.BatchNo } equals new { ProductionOrderId = r.ProductionOrderId.Value, r.BatchNo } into rr
                                                from r in rr.DefaultIfEmpty()
                                                join d in DBContext.ProductionOrderDetails on b.OrderDetailsId equals d.OrderDetailsId
                                                join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                                join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                                join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                                join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                                where ln.ProductionLineCode == ProductionLineCode && ln.PlantCode == PlantCode
                                                select new ProductionLineDetailsParam
                                                {
                                                    PlantId = x.PlantId,
                                                    PlantCode = x.PlantCode,
                                                    PlantDesc = x.PlantDesc,
                                                    OrderTypeId = o.OrderTypeId,
                                                    OrderTypeCode = o.OrderTypeCode,
                                                    OrderTypeDesc = o.OrderTypeDesc,
                                                    ProductId = p.ProductId,
                                                    ProductCode = p.ProductCode,
                                                    ProductDesc = (applang == "ar") ? p.ProductDescAr : p.ProductDesc,
                                                    ProductionOrderId = b.ProductionOrderId,
                                                    SapOrderId = b.SapOrderId,
                                                    ProductionOrderQty = l.Qty,
                                                    ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    OrderDate = l.OrderDate,
                                                    OrderDetailsId = b.OrderDetailsId,
                                                    BatchNo = d.BatchNo,
                                                    BatchQty = d.Qty,
                                                    BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    NoCanPerCartoon = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0)) : (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                                                    NoCartoonPerPallet = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) : (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0)),
                                                    ProductionDate = d.ProductionDate,
                                                    IsClosedBatch = d.IsClosedBatch,
                                                    IsReleased = d.IsReleased,
                                                    BatchStatus = d.BatchStatus,
                                                    IsReceived = d.IsReceived,
                                                    ReceivedQty = d.ReceivedQty,
                                                    ReceivedQtyCartoon = (r.BatchNo != null) ? (d.ReceivedQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    ProductionLineId = ln.ProductionLineId,
                                                    ProductionLineCode = ln.ProductionLineCode,
                                                    ProductionLineDesc = ln.ProductionLineDesc
                                                }).Distinct().ToList();

                productionLineDetails = (from b in GetProductionLineDetails
                                         select new ProductionLineDetailsParam
                                         {
                                             PlantId = b.PlantId,
                                             PlantCode = b.PlantCode,
                                             PlantDesc = b.PlantDesc,
                                             OrderTypeId = b.OrderTypeId,
                                             OrderTypeCode = b.OrderTypeCode,
                                             OrderTypeDesc = b.OrderTypeDesc,
                                             ProductId = b.ProductId,
                                             ProductCode = b.ProductCode,
                                             ProductDesc = b.ProductDesc,
                                             ProductionOrderId = b.ProductionOrderId,
                                             SapOrderId = b.SapOrderId,
                                             ProductionOrderQty = b.ProductionOrderQty,
                                             ProductionOrderQtyCartoon = b.ProductionOrderQtyCartoon,
                                             OrderDate = b.OrderDate,
                                             OrderDetailsId = b.OrderDetailsId,
                                             BatchNo = b.BatchNo,
                                             BatchQty = b.BatchQty,
                                             BatchQtyCartoon = b.BatchQtyCartoon,
                                             NoCanPerCartoon = b.NoCanPerCartoon,
                                             NoCartoonPerPallet = b.NoCartoonPerPallet,
                                             PalletCapacity = (b.NoCartoonPerPallet * b.NoCanPerCartoon),
                                             ProductionDate = b.ProductionDate,
                                             IsClosedBatch = b.IsClosedBatch,
                                             IsReleased = b.IsReleased,
                                             BatchStatus = b.BatchStatus,
                                             IsReceived = b.IsReceived,
                                             ReceivedQty = b.ReceivedQty,
                                             ReceivedQtyCartoon = b.ReceivedQtyCartoon,
                                             ProductionLineId = b.ProductionLineId,
                                             ProductionLineCode = b.ProductionLineCode,
                                             ProductionLineDesc = b.ProductionLineDesc
                                         }).Distinct().ToList();

                if (productionLineDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, productionLineDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetProductionLineDetails3(long UserID, string DeviceSerialNo, string applang, string PlantCode, string ProductionLineCode, string BatchNo)
        {
            ResponseStatus responseStatus = new();
            ProductionLineDetailsParam productionLineDetails = new();
            try
            {
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, productionLineDetails };
                }
                if (ProductionLineCode == null || string.IsNullOrEmpty(ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                    return new { responseStatus, productionLineDetails };
                }
                if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == ProductionLineCode && x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود خط الإنتاج لا ينتمي للمصنع" : "Production Line code is not related to the plant");
                    return new { responseStatus, productionLineDetails };
                }
                if (BatchNo == null || string.IsNullOrEmpty(BatchNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الباتش" : "Missing batch no");
                    return new { responseStatus, productionLineDetails };
                }

                var GetProductionLineDetails = (from b in DBContext.ProductionLineWips
                                                join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                                join r in DBContext.ProductionOrderReceivings on new { b.ProductionOrderId, b.BatchNo } equals new { ProductionOrderId = r.ProductionOrderId.Value, r.BatchNo } into rr
                                                from r in rr.DefaultIfEmpty()
                                                join d in DBContext.ProductionOrderDetails on b.OrderDetailsId equals d.OrderDetailsId
                                                join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                                join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                                join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                                join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                                where ln.ProductionLineCode == ProductionLineCode && ln.PlantCode == PlantCode && d.BatchNo == BatchNo
                                                select new ProductionLineDetailsParam
                                                {
                                                    PlantId = x.PlantId,
                                                    PlantCode = x.PlantCode,
                                                    PlantDesc = x.PlantDesc,
                                                    OrderTypeId = o.OrderTypeId,
                                                    OrderTypeCode = o.OrderTypeCode,
                                                    OrderTypeDesc = o.OrderTypeDesc,
                                                    ProductId = p.ProductId,
                                                    ProductCode = p.ProductCode,
                                                    ProductDesc = (applang == "ar") ? p.ProductDescAr : p.ProductDesc,
                                                    ProductionOrderId = b.ProductionOrderId,
                                                    SapOrderId = b.SapOrderId,
                                                    ProductionOrderQty = l.Qty,
                                                    ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    OrderDate = l.OrderDate,
                                                    OrderDetailsId = b.OrderDetailsId,
                                                    BatchNo = d.BatchNo,
                                                    BatchQty = d.Qty,
                                                    BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    NoCanPerCartoon = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0)) : (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                                                    NoCartoonPerPallet = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) : (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0)),
                                                    ProductionDate = d.ProductionDate,
                                                    IsClosedBatch = d.IsClosedBatch,
                                                    IsReleased = d.IsReleased,
                                                    BatchStatus = d.BatchStatus,
                                                    IsReceived = d.IsReceived,
                                                    ReceivedQty = d.ReceivedQty,
                                                    ReceivedQtyCartoon = (r.BatchNo != null) ? (d.ReceivedQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    ProductionLineId = ln.ProductionLineId,
                                                    ProductionLineCode = ln.ProductionLineCode,
                                                    ProductionLineDesc = ln.ProductionLineDesc
                                                }).Distinct().ToList();

                productionLineDetails = (from b in GetProductionLineDetails
                                         select new ProductionLineDetailsParam
                                         {
                                             PlantId = b.PlantId,
                                             PlantCode = b.PlantCode,
                                             PlantDesc = b.PlantDesc,
                                             OrderTypeId = b.OrderTypeId,
                                             OrderTypeCode = b.OrderTypeCode,
                                             OrderTypeDesc = b.OrderTypeDesc,
                                             ProductId = b.ProductId,
                                             ProductCode = b.ProductCode,
                                             ProductDesc = b.ProductDesc,
                                             ProductionOrderId = b.ProductionOrderId,
                                             SapOrderId = b.SapOrderId,
                                             ProductionOrderQty = b.ProductionOrderQty,
                                             ProductionOrderQtyCartoon = b.ProductionOrderQtyCartoon,
                                             OrderDate = b.OrderDate,
                                             OrderDetailsId = b.OrderDetailsId,
                                             BatchNo = b.BatchNo,
                                             BatchQty = b.BatchQty,
                                             BatchQtyCartoon = b.BatchQtyCartoon,
                                             NoCanPerCartoon = b.NoCanPerCartoon,
                                             NoCartoonPerPallet = b.NoCartoonPerPallet,
                                             PalletCapacity = (b.NoCartoonPerPallet * b.NoCanPerCartoon),
                                             ProductionDate = b.ProductionDate,
                                             IsClosedBatch = b.IsClosedBatch,
                                             IsReleased = b.IsReleased,
                                             BatchStatus = b.BatchStatus,
                                             IsReceived = b.IsReceived,
                                             ReceivedQty = b.ReceivedQty,
                                             ReceivedQtyCartoon = b.ReceivedQtyCartoon,
                                             ProductionLineId = b.ProductionLineId,
                                             ProductionLineCode = b.ProductionLineCode,
                                             ProductionLineDesc = b.ProductionLineDesc
                                         }).FirstOrDefault();

                if (productionLineDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, productionLineDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPalletDetails(long UserID, string DeviceSerialNo, string applang, string PalletCode, string ProductCode)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            decimal NoCanPerCartoon = 0;
            decimal NoCartoonPerPallet = 0;
            decimal PalletCapacity = 0;
            try
            {
                if (ProductCode == null || string.IsNullOrEmpty(ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الصنف" : "Missing product code");
                    return new { responseStatus, palletDetails };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت التي فى المخزن خاطئ" : "Warehouse Pallet Code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الصنف خاطئ" : "Wrong product code");
                    return new { responseStatus, palletDetails };
                }
                var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == ProductCode);

                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت غير مستخدم" : "Pallet code is not used");

                    NoCartoonPerPallet = ((ProductData.NumeratorforConversionPal ?? 0) / (ProductData.DenominatorforConversionPal ?? 0));
                    NoCanPerCartoon = ((ProductData.NumeratorforConversionPac ?? 0) / (ProductData.DenominatorforConversionPac ?? 0));
                    palletDetails = new()
                    {
                        AvailableQty = NoCartoonPerPallet * NoCanPerCartoon,
                        AvailableQtyCarton = NoCartoonPerPallet,
                        PalletCode = PalletCode
                    };
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.ProductCode == ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الصنف غير موجود على الباليت" : "Product code is not exist in the specified pallet");
                    return new { responseStatus, palletDetails };
                }


                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);

                var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode);
                if (ProductionOrderReceivingsData != null)
                {
                    NoCanPerCartoon = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));
                    NoCartoonPerPallet = ((ProductionOrderReceivingsData.NumeratorforConversionPal ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPal ?? 0));
                    PalletCapacity = NoCartoonPerPallet * NoCanPerCartoon;
                }

                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join d in DBContext.ProductionOrderDetails on new
                                 {
                                     ProductionOrderId = b.ProductionOrderId.Value,
                                     BatchNo = b.BatchNo,
                                     ProductionLineId = b.ProductionLineId.Value
                                 } equals new
                                 {
                                     d.ProductionOrderId,
                                     d.BatchNo,
                                     ProductionLineId = d.ProductionLineId.Value
                                 }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 join na in DBContext.ReceivingPalletsNeedApprovals on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { na.PalletCode, na.ProductionOrderId, na.BatchNo, na.ProductionLineId } into naa
                                 from na in naa.DefaultIfEmpty()
                                 where b.PalletCode == PalletCode
                                 select new PalletDetailsParam
                                 {
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletQty = r.Qty,
                                     PalletCode = b.PalletCode,
                                     PalletCartoonQty = Math.Round((r.Qty / NoCanPerCartoon).Value, 2),
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = (b.IsChangedQuantityByWarehouse == true) ? (na.WarehouseReceivingQty) : b.WarehouseReceivingQty,
                                     WarehouseReceivingCartoonQty = (b.IsChangedQuantityByWarehouse == true) ? (Convert.ToDecimal(na.WarehouseCartoonReceivingQty)) : ((b.WarehouseReceivingQty > 0) ? Math.Round((b.WarehouseReceivingQty / NoCanPerCartoon).Value, 2) : 0),
                                     LaneCode = b.LaneCode,
                                     AvailableQtyCarton = (long)((PalletCapacity - r.Qty) / NoCanPerCartoon),
                                     AvailableQty = (long)(PalletCapacity - r.Qty),
                                     ProductionLineId = ln.ProductionLineId,
                                     ProductionLineCode = ln.ProductionLineCode,
                                     ProductionLineDesc = ln.ProductionLineDesc,
                                     BatchQty = d.Qty,
                                     BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     ProductionDate = b.ProductionDate,
                                     ReceivedQty = b.ReceivingQty,
                                     ReceivedQtyCartoon = (r.BatchNo != null) ? (b.ReceivingQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (b.ReceivingQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     IsChangedQuantityByWarehouse = b.IsChangedQuantityByWarehouse,
                                     IsProductionTakeAction = b.IsProductionTakeAction
                                 }).FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت غير مستخدم" : "Pallet code is not used");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPalletDetails2(long UserID, string DeviceSerialNo, string applang, string PalletCode)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            try
            {
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت التي فى المخزن خاطئ" : "Warehouse Pallet Code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت غير مستخدم" : "Pallet code is not used");
                    return new { responseStatus, palletDetails };
                }
                decimal NoCanPerCartoon = 0;
                decimal NoCartoonPerPallet = 0;
                decimal PalletCapacity = 0;
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);

                var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode);
                if (ProductionOrderReceivingsData != null)
                {
                    NoCanPerCartoon = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));
                    NoCartoonPerPallet = ((ProductionOrderReceivingsData.NumeratorforConversionPal ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPal ?? 0));
                    PalletCapacity = NoCartoonPerPallet * NoCanPerCartoon;
                }

                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join d in DBContext.ProductionOrderDetails on new
                                 {
                                     ProductionOrderId = b.ProductionOrderId.Value,
                                     BatchNo = b.BatchNo,
                                     ProductionLineId = b.ProductionLineId.Value
                                 } equals new
                                 {
                                     d.ProductionOrderId,
                                     d.BatchNo,
                                     ProductionLineId = d.ProductionLineId.Value
                                 }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join lane in DBContext.Lanes on b.LaneCode equals lane.LaneCode into laneDT
                                 from lane in laneDT.DefaultIfEmpty()
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.PalletCode == PalletCode
                                 select new PalletDetailsParam
                                 {
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletCode = b.PalletCode,
                                     PalletQty = r.Qty,
                                     PalletCartoonQty = Math.Round((r.Qty / NoCanPerCartoon).Value, 2),
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     WarehouseReceivingCartoonQty = ((b.WarehouseReceivingQty > 0) ? Math.Round((b.WarehouseReceivingQty / NoCanPerCartoon).Value, 2) : 0),
                                     LaneCode = b.LaneCode,
                                     LaneDesc = lane.LaneDesc,
                                     AvailableQty = (long)(NoCartoonPerPallet - Math.Round((r.Qty / NoCanPerCartoon).Value, 2)),
                                     ProductionLineId = ln.ProductionLineId,
                                     ProductionLineCode = ln.ProductionLineCode,
                                     ProductionLineDesc = ln.ProductionLineDesc,
                                     BatchQty = d.Qty,
                                     BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     ProductionDate = b.ProductionDate,
                                     NoCanPerCartoon = NoCanPerCartoon,
                                     NoCartoonPerPallet = NoCartoonPerPallet,
                                     PalletCapacity = PalletCapacity,
                                     ReceivedQty = b.ReceivingQty,
                                     ReceivedQtyCartoon = (r.BatchNo != null) ? (b.ReceivingQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (b.ReceivingQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     IsProductionTakeAction = b.IsProductionTakeAction,
                                     IsChangedQuantityByWarehouse = b.IsChangedQuantityByWarehouse,

                                 }).FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت غير مستخدم" : "Pallet code is not used");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }

        
        [Route("[action]")]
        [HttpPost]
        public object ProductionReceiving(long UserID,string DeviceSerialNo,string applang,string PlantCode, string ProductCode,int OrderDetailsId,int CartonsReceivedQty, string PalletCode, bool? IsExcessProductionReceiving, string StorageLocationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(PlantCode) || string.IsNullOrWhiteSpace(PlantCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing plant code", "لم يتم إدخال كود المصنع", applang) };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Plant code is wrong", "كود المصنع خاطئ", applang) };
                }
                if (string.IsNullOrEmpty(StorageLocationCode) || string.IsNullOrWhiteSpace(StorageLocationCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing storage location code", "لم يتم إدخال كود موقع التخزين", applang) };
                }
                if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == StorageLocationCode && x.PlantCode == PlantCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Storage location code is wrong", "كود موقع التخزين خاطئ", applang)};
                }
                if (string.IsNullOrEmpty(ProductCode) || string.IsNullOrWhiteSpace(ProductCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing Product Code", "لم يتم إدخال كود المنتج", applang)};
                }
                var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == ProductCode);
                int NoItemPerBox = 0;
                int NoBoxPerPallet = 0;
                int AvailablePalletReceivingCartons = 0;
                int MaxReceivingCartons = 0;
                if (ProductData == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Product code is wrong", "كود المنتج خاطئ", applang)};
                }
                else
                {
                    NoItemPerBox = (int)((ProductData.NumeratorforConversionPac ?? 0) / (ProductData.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = (int)((ProductData.NumeratorforConversionPal ?? 0) / (ProductData.DenominatorforConversionPal ?? 0));
                }
                if (CartonsReceivedQty <= 0)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Cartons Received Qty must be greater than zero", "يجب أن تكون كمية الكراتين المستلمة أكبر من الصفر", applang) };
                }
                if (CartonsReceivedQty > NoBoxPerPallet)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus($"Cartons Received Qty must not exceed pallet capacity {NoBoxPerPallet}", $"يجب أن تكون كمية الكراتين المستلمة لا تتخطى سعة الباليت وهي {NoBoxPerPallet}", applang) };
                }

                if (string.IsNullOrEmpty(PalletCode) || string.IsNullOrWhiteSpace(PalletCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing pallet code", "لم يتم إدخال كود الباليت", applang)};
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Warehouse Pallet Code is wrong", "كود الباليت التي فى المخزن خاطئ", applang) };
                }

                var OrderDetailsData = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.OrderDetailsId == OrderDetailsId && x.ProductCode == ProductCode && x.PlantCode == PlantCode && x.IsReleased == true);
                if (OrderDetailsData == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Order Details Id is wrong", "رقم تفاصيل أمر الإنتاج خاطئ", applang) };
                }
                var expectedTotalReceivingCartonsQty = 0;
                var expectedReceivedQtyUnits = 0;

                var PalletWip = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (PalletWip == null)
                {
                    PalletWip = new()
                    {
                        PalletCode = PalletCode,
                        ProductCode = ProductCode,
                        ProductionOrderId = OrderDetailsData.ProductionOrderId,
                        SaporderId = OrderDetailsData.SapOrderId,
                        PlantCode = PlantCode,
                        ProductionDate = OrderDetailsData.ProductionDate,
                        BatchNo = OrderDetailsData.BatchNo,
                        ProductionLineId = OrderDetailsData.ProductionLineId,
                        ReceivingQty = CartonsReceivedQty * NoItemPerBox,
                        IsWarehouseLocation = ProductData.IsWarehouseLocation,

                    };
                    DBContext.PalletWips.Add(PalletWip);
                    ProductionOrderReceiving productionOrderReceiving = new()
                    {
                        BatchNo = OrderDetailsData.BatchNo,
                        NumeratorforConversionPac = ProductData.NumeratorforConversionPac,
                        NumeratorforConversionPal = ProductData.NumeratorforConversionPal,
                        DenominatorforConversionPac = ProductData.DenominatorforConversionPac,
                        DenominatorforConversionPal = ProductData.DenominatorforConversionPal,
                        DateTimeAdd = DateTime.Now,
                        UserIdAdd = UserID,
                        ProductionDate = OrderDetailsData.ProductionDate,
                        DeviceSerialNoReceived = DeviceSerialNo,
                        IsWarehouseReceived = false,
                        ProductCode = OrderDetailsData.ProductCode,
                        PalletCode = PalletCode,
                        PlantCode = OrderDetailsData.PlantCode,
                        ProductionLineId = OrderDetailsData.ProductionLineId,
                        ProductionOrderId = OrderDetailsData.ProductionOrderId,
                        Qty = CartonsReceivedQty * NoItemPerBox,
                        SaporderId = OrderDetailsData.SapOrderId,
                        IsExcessProductionReceiving = IsExcessProductionReceiving
                    };
                    DBContext.ProductionOrderReceivings.Add(productionOrderReceiving);
                }
                else
                {
                    AvailablePalletReceivingCartons = NoBoxPerPallet - (int)((PalletWip.ReceivingQty ?? 0) / NoBoxPerPallet);
                    if (CartonsReceivedQty <= AvailablePalletReceivingCartons)
                    {
                        PalletWip.ReceivingQty = CartonsReceivedQty * NoItemPerBox;
                        DBContext.PalletWips.Update(PalletWip);
                        var ProductionOrderReceivingData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.ProductionOrderId == PalletWip.ProductionOrderId && x.BatchNo == PalletWip.BatchNo && x.ProductCode == PalletWip.ProductCode && x.ProductionLineId == PalletWip.ProductionLineId);
                        ProductionOrderReceivingData.Qty = CartonsReceivedQty * NoItemPerBox;
                        DBContext.ProductionOrderReceivings.Update(ProductionOrderReceivingData);
                    }
                    else
                    {
                        return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Exceeding the maximum receiving quantity for the pallet", "تجاوز الحد الأقصى لكمية الاستلام للباليت", applang) };
                    }

                }
               
                
                    var receivedQtyUnits = OrderDetailsData.ReceivedQty;
                    var batchQtyUnits = OrderDetailsData.Qty;
                    expectedReceivedQtyUnits = (int)  receivedQtyUnits + (CartonsReceivedQty * NoItemPerBox);
                    expectedTotalReceivingCartonsQty = (int)(expectedReceivedQtyUnits / NoItemPerBox);

                    if (expectedReceivedQtyUnits > batchQtyUnits)
                    {
                        if (!IsExcessProductionReceiving.HasValue || IsExcessProductionReceiving == false)
                        {
                            return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Exceeding the maximum receiving quantity for the order details", "تجاوز الحد الأقصى لكمية الاستلام لتفاصيل الطلب", applang) };
                        }
                    }
                    
                Console.WriteLine($"receivedQty:{OrderDetailsData.ReceivedQty} expectedReceivedQtyUnits: {expectedReceivedQtyUnits}, expectedTotalReceivingCartonsQty: {expectedTotalReceivingCartonsQty}, batchQtyUnits: {batchQtyUnits}, NoItemPerBox: {NoItemPerBox}");

                OrderDetailsData.ReceivedQty = expectedReceivedQtyUnits;
                OrderDetailsData.BatchStatus = "Receiving";
                OrderDetailsData.DateTimeUpdate = DateTime.Now;
                OrderDetailsData.UserIdUpdate = UserID;
                OrderDetailsData.DeviceSerialNoUpdate = DeviceSerialNo;
                if (OrderDetailsData.ReceivedQty >= OrderDetailsData.Qty)
                {
                    OrderDetailsData.IsReceived = true;
                }
                DBContext.ProductionOrderDetails.Update(OrderDetailsData);
                
                DBContext.SaveChanges();

                var OrderDetailsDataAfterSave = (from pod in DBContext.ProductionOrderDetails
                                                 join po in DBContext.ProductionOrders on pod.ProductionOrderId equals po.ProductionOrderId
                                                 join p in DBContext.Products on pod.ProductCode equals p.ProductCode
                                                 join pl in DBContext.Plants on pod.PlantCode equals pl.PlantCode
                                                 join prl in DBContext.ProductionLines on pod.ProductionLineId equals prl.ProductionLineId
                                                 join ol in DBContext.OrderTypes on po.OrderTypeCode equals ol.OrderTypeCode                                             
                                                 where pod.OrderDetailsId == OrderDetailsId
                                                 select new ProductionLineDetailsParam
                                                 {
                                                     PlantId = pl.PlantId,
                                                     PlantCode = pl.PlantCode,
                                                     PlantDesc = pl.PlantDesc,
                                                     OrderTypeId = ol.OrderTypeId,
                                                     OrderTypeCode = ol.OrderTypeCode,
                                                     OrderTypeDesc = ol.OrderTypeDesc,
                                                     ProductId = p.ProductId,
                                                     ProductCode = p.ProductCode,
                                                     ProductDesc = p.ProductDesc,
                                                        ProductionOrderId = po.ProductionOrderId,
                                                        SapOrderId = po.SapOrderId.Value,
                                                        ProductionOrderQty = po.Qty,
                                                        ProductionOrderQtyCartoon = (po.Qty / NoItemPerBox),
                                                        OrderDate = po.OrderDate,
                                                        OrderDetailsId = pod.OrderDetailsId,
                                                        BatchNo = pod.BatchNo,
                                                        BatchQty = pod.Qty,
                                                        BatchQtyCartoon = (pod.Qty / NoItemPerBox),
                                                        NoCanPerCartoon = NoItemPerBox,
                                                        NoCartoonPerPallet = NoBoxPerPallet,
                                                        PalletCapacity = (NoBoxPerPallet * NoItemPerBox),
                                                        ProductionDate = pod.ProductionDate,
                                                        IsClosedBatch = pod.IsClosedBatch,
                                                        IsReleased = pod.IsReleased,
                                                        BatchStatus = pod.BatchStatus,
                                                        IsReceived = pod.IsReceived,
                                                        ReceivedQty = pod.ReceivedQty,
                                                        ReceivedQtyCartoon = (pod.ReceivedQty / NoItemPerBox),
                                                        ProductionLineId = pod.ProductionLineId.Value,
                                                        ProductionLineCode = prl.ProductionLineCode,
                                                        ProductionLineDesc = prl.ProductionLineDesc,
                                                        }).FirstOrDefault();

            

                //var GetProductionLineDetails = (from b in DBContext.ProductionLineWips
                //                                join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                //                                join r in DBContext.ProductionOrderReceivings on new { b.ProductionOrderId, b.BatchNo } equals new { ProductionOrderId = r.ProductionOrderId.Value, r.BatchNo } into rr
                //                                from r in rr.DefaultIfEmpty()
                //                                join d in DBContext.ProductionOrderDetails on b.OrderDetailsId equals d.OrderDetailsId
                //                                join p in DBContext.Products on l.ProductCode equals p.ProductCode
                //                                join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                //                                join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                //                                join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                //                                where ln.ProductionLineId == OrderDetailsData.ProductionLineId && ln.PlantCode == PlantCode
                //                                select new ProductionLineDetailsParam
                //                                {
                //                                    PlantId = x.PlantId,
                //                                    PlantCode = x.PlantCode,
                //                                    PlantDesc = x.PlantDesc,
                //                                    OrderTypeId = o.OrderTypeId,
                //                                    OrderTypeCode = o.OrderTypeCode,
                //                                    OrderTypeDesc = o.OrderTypeDesc,
                //                                    ProductId = p.ProductId,
                //                                    ProductCode = p.ProductCode,
                //                                    ProductDesc = p.ProductDesc,
                //                                    ProductionOrderId = b.ProductionOrderId,
                //                                    SapOrderId = b.SapOrderId,
                //                                    ProductionOrderQty = l.Qty,
                //                                    ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                //                                    OrderDate = l.OrderDate,
                //                                    OrderDetailsId = b.OrderDetailsId,
                //                                    BatchNo = d.BatchNo,
                //                                    BatchQty = d.Qty,
                //                                    BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                //                                    NoCanPerCartoon = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0)) : (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                //                                    NoCartoonPerPallet = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) : (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0)),
                //                                    ProductionDate = d.ProductionDate,
                //                                    IsClosedBatch = d.IsClosedBatch,
                //                                    IsReleased = d.IsReleased,
                //                                    BatchStatus = d.BatchStatus,
                //                                    IsReceived = d.IsReceived,
                //                                    ReceivedQty = d.ReceivedQty,
                //                                    ReceivedQtyCartoon = (r.BatchNo != null) ? (d.ReceivedQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                //                                    ProductionLineId = ln.ProductionLineId,
                //                                    ProductionLineCode = ln.ProductionLineCode,
                //                                    ProductionLineDesc = ln.ProductionLineDesc
                //                                }).Distinct().ToList();


                //var productionLineDetails = (from b in GetProductionLineDetails
                //                             select new ProductionLineDetailsParam
                //                             {
                //                                 PlantId = b.PlantId,
                //                                 PlantCode = b.PlantCode,
                //                                 PlantDesc = b.PlantDesc,
                //                                 OrderTypeId = b.OrderTypeId,
                //                                 OrderTypeCode = b.OrderTypeCode,
                //                                 OrderTypeDesc = b.OrderTypeDesc,
                //                                 ProductId = b.ProductId,
                //                                 ProductCode = b.ProductCode,
                //                                 ProductDesc = b.ProductDesc,
                //                                 ProductionOrderId = b.ProductionOrderId,
                //                                 SapOrderId = b.SapOrderId,
                //                                 ProductionOrderQty = b.ProductionOrderQty,
                //                                 ProductionOrderQtyCartoon = b.ProductionOrderQtyCartoon,
                //                                 OrderDate = b.OrderDate,
                //                                 OrderDetailsId = b.OrderDetailsId,
                //                                 BatchNo = b.BatchNo,
                //                                 BatchQty = b.BatchQty,
                //                                 BatchQtyCartoon = b.BatchQtyCartoon,
                //                                 NoCanPerCartoon = b.NoCanPerCartoon,
                //                                 NoCartoonPerPallet = b.NoCartoonPerPallet,
                //                                 PalletCapacity = (b.NoCartoonPerPallet * b.NoCanPerCartoon),
                //                                 ProductionDate = b.ProductionDate,
                //                                 IsClosedBatch = b.IsClosedBatch,
                //                                 IsReleased = b.IsReleased,
                //                                 BatchStatus = b.BatchStatus,
                //                                 IsReceived = b.IsReceived,
                //                                 ReceivedQty = b.ReceivedQty,
                //                                 ReceivedQtyCartoon = b.ReceivedQtyCartoon,
                //                                 ProductionLineId = b.ProductionLineId,
                //                                 ProductionLineCode = b.ProductionLineCode,
                //                                 ProductionLineDesc = b.ProductionLineDesc
                //                             }).FirstOrDefault();
                if (OrderDetailsDataAfterSave != null)
                {
                    return new
                    {
                        responseStatus = ResponseStatusHelper.SuccessResponseStatus("Data sent successfully", "تم إرسال البيانات بنجاح", applang),
                        productionLineDetails = OrderDetailsDataAfterSave,
                        palletDetails = new PalletDetailsParam()
                    };
                }
                else
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Error in getting production line details", "حدث خطأ في الحصول على تفاصيل خط الإنتاج", applang) };

                }
            }
            catch (Exception ex)
            {
                return new { ResponseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.InnerException?.Message, applang) };
                Console.WriteLine(ex.InnerException?.Message);
            }
            
        }
        [Route("[action]")]
        [HttpPut]
        public async Task<object> UpdateBatchAsync(long? ProductionOrderDetailsId, string applang, int? newBatchQuantity,string batchNo, DateTime? productionDate)
        {
            try
            {
                if (ProductionOrderDetailsId == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("ProductionOrderDetailsId is missing!", "معرف تفاصيل طلب الانتاج غير موجود", applang) };
                }
                if (newBatchQuantity == null) {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("New batch quantity is missing", "كمية الباتش الجديدة غير موجودة!", applang) };
                }
                if(newBatchQuantity <= 0)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("New batch quantity must be greater than zero!", "يجب أن تكون كمية الباتش الجديدة أكبر من الصفر!", applang) };
                }
                
                if (productionDate != null)
                {
                    if (string.IsNullOrEmpty(batchNo) || string.IsNullOrWhiteSpace(batchNo))
                    {
                        return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Batch number is missing", "رقم الباتش غير موجود!", applang) };
                    }
                }
                var orderDetails = (from o in DBContext.ProductionOrderDetails
                                    join p in DBContext.Products on o.ProductCode equals p.ProductCode
                                    where o.OrderDetailsId == ProductionOrderDetailsId 
                                    select new
                                    {
                                        OrderDetailsId = o.OrderDetailsId,
                                        ProductCode = p.ProductCode,
                                        NoItemPerBox = (int)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                                        BatchQty = o.Qty,
                                        BatchQtyCartons =(int) (o.Qty / (int)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                        ReceivedQty = o.ReceivedQty,
                                        ReceivedQtyCartons = (int)(o.ReceivedQty / (int)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                        IsClosed = o.IsClosedBatch,
                                        WarehouseReceivedQty = DBContext.PalletWips.Where(b => b.ProductionOrderId == o.ProductionOrderId && b.BatchNo == o.BatchNo && b.ProductCode == o.ProductCode).Sum(b => b.WarehouseReceivingQty) ?? 0,
                                    }).FirstOrDefault();
                if (orderDetails == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Wrong ProductionOrderDetailsId!", "خطأ في معرف تفاصيل طلب الانتاج", applang) };
                }
                if(newBatchQuantity < orderDetails.ReceivedQtyCartons)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("New batch quantity must be greater than or equal received quantity!", "يجب أن تكون كمية الباتش الجديدة أكبر من أو تساوي الكمية المستلمة!", applang) };
                }
                if (orderDetails.WarehouseReceivedQty > 0)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Can not edit batch that has warehouse received quantity!", "لا يمكن تعديل باتش به كمية مستلمة من المخزن بالفعل!", applang) };
                }
                var orderDetailsToUpdate = DBContext.ProductionOrderDetails.Find(ProductionOrderDetailsId);
                var oldBatchNo = orderDetailsToUpdate.BatchNo;
                orderDetailsToUpdate.Qty = newBatchQuantity * orderDetails.NoItemPerBox;
                orderDetailsToUpdate.BatchNo = batchNo;
                orderDetailsToUpdate.ProductionDate = productionDate;
                DBContext.ProductionOrderDetails.Update(orderDetailsToUpdate);
                var batchesToUpdate = DBContext.BatchLists.FirstOrDefault(b => b.ProductionOrderDetailsId == ProductionOrderDetailsId);
                if (batchesToUpdate != null)
                {
                    batchesToUpdate.QtyProduction = newBatchQuantity * orderDetails.NoItemPerBox;
                    batchesToUpdate.BatchNo = batchNo;
                    batchesToUpdate.ProductionDate = productionDate;
                    DBContext.BatchLists.Update(batchesToUpdate);
                }
                await DBContext.PalletWips
                        .Where(r=>r.BatchNo == oldBatchNo)
                        .ExecuteUpdateAsync(
                    r=>r.SetProperty(r=>r.BatchNo,batchNo)
                    .SetProperty(r=>r.ProductionDate,productionDate)
                    );
                await DBContext.ProductionOrderReceivings
                        .Where(r => r.BatchNo == oldBatchNo)
                        .ExecuteUpdateAsync(
                    r => r.SetProperty(r => r.BatchNo, batchNo)
                    .SetProperty(r => r.ProductionDate,productionDate)
                    );



                var orderDetailsAfterUpdate = (
                    from o in DBContext.ProductionOrderDetails
                    join p in DBContext.Products on o.ProductCode equals p.ProductCode
                    join pl in DBContext.ProductionLines on o.ProductionLineId equals pl.ProductionLineId
                    where o.OrderDetailsId == ProductionOrderDetailsId
                    select new
                    {
                        orderDetailsId = o.OrderDetailsId,
                        qty = o.Qty,
                        qtyCartoon = (int)(o.Qty / (p.NumeratorforConversionPac / p.DenominatorforConversionPac)),
                        batchNo = o.BatchNo,
                        productionLineId = o.ProductionLineId,
                        productionLineCode = pl.ProductionLineCode,
                        productionDate = o.ProductionDate,
                        isClosedBatch = o.IsClosedBatch,
                        productionOrderId = o.ProductionOrderId,
                        isReleased = o.IsReleased,
                        batchStatus = o.BatchStatus
                    }
                ).FirstOrDefault();

                DBContext.SaveChanges();

                return new {    
                    ResponseStatus = ResponseStatusHelper.SuccessResponseStatus("Batch quantity updated successfully", "تم تحديث كمية الباتش بنجاح", applang),
                    ProductionOrderDetails = orderDetailsAfterUpdate,
                    };
                }
            catch (Exception ex)
            {
                return new { ResponseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.InnerException?.Message, applang) };
            }
        }
        [Route("[action]")]
        [HttpDelete]
        public object DeleteBatch(long? ProductionOrderDetailsId, string applang)
        {
            try
            {
                if (ProductionOrderDetailsId == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("ProductionOrderDetailsId is missing!", "معرف تفاصيل طلب الانتاج غير موجود", applang) };
                }
                var orderDetails = DBContext.ProductionOrderDetails.Find(ProductionOrderDetailsId);
                if (orderDetails == null)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Wrong ProductionOrderDetailsId!", "خطأ في معرف تفاصيل طلب الانتاج", applang) };
                }
                if (orderDetails.ReceivedQty != 0)
                {
                    return new { ResponseStatus = ResponseStatusHelper.ErrorResponseStatus("Can not delete batch that has received quantity!", "لا يمكن حذف باتش به كمية مستلمة بالفعل!", applang) };
                }
                DBContext.ProductionOrderDetails.Remove(orderDetails);
                var batch = DBContext.BatchLists.FirstOrDefault(b => b.ProductionOrderDetailsId == ProductionOrderDetailsId);
                if (batch != null)
                {
                    DBContext.BatchLists.Remove(batch);
                }
                DBContext.SaveChanges();
                return new { ResponseStatus = ResponseStatusHelper.SuccessResponseStatus("Batch deleted successfully", "تم إزالة الباتش بنجاح", applang) };
            } catch(Exception ex)
            {
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return new { ResponseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.Message,applang) };
                
            }
        }
        [Route("[action]")]
        [HttpGet]
        public async Task<object> ProductionReceivingAsync(long UserID, string DeviceSerialNo, string applang, string PlantCode, int ProductionOrderDetailsId, string ProductCode, long? CartoonReceivedQty, string PalletCode, bool? IsExcessProductionReceiving, string StorageLocation)
        {
            ResponseStatus responseStatus = new();
            ProductionLineDetailsParam productionLineDetails = new();
            PalletDetailsParam palletDetails = new();
            try
            {
                if (CartoonReceivedQty == null || string.IsNullOrEmpty(CartoonReceivedQty.ToString().Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية المستلمه" : "Missing Cartoon Received Qty");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                //if (ProductionLineCode == null || string.IsNullOrEmpty(ProductionLineCode.Trim()))
                //{
                //    responseStatus.StatusCode = 401;
                //    responseStatus.IsSuccess = false;
                //    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود خط الإنتاج" : "Missing production line code");
                //    return new { responseStatus, productionLineDetails, palletDetails };
                //}
                //if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == ProductionLineCode && x.PlantCode == PlantCode))
                //{
                //    responseStatus.StatusCode = 401;
                //    responseStatus.IsSuccess = false;
                //    responseStatus.StatusMessage = ((applang == "ar") ? "كود خط الإنتاج لا ينتمي للمصنع" : "Production Line code is not related to the plant");
                //    return new { responseStatus, productionLineDetails, palletDetails };
                //}
               // var ProductionLineData = DBContext.ProductionLines.FirstOrDefault(x => x.ProductionLineCode == ProductionLineCode && x.PlantCode == PlantCode);
                var ProductionLineWipData = DBContext.ProductionLineWips.FirstOrDefault(x => x.OrderDetailsId == ProductionOrderDetailsId && x.PlantCode == PlantCode);
                if (ProductionLineWipData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                if (ProductCode == null || string.IsNullOrEmpty(ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing Product Code");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == ProductCode);
                if (ProductData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                var ProductionOrderDetailsData = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.OrderDetailsId == ProductionOrderDetailsId && x.BatchNo == ProductionLineWipData.BatchNo && x.ProductionOrderId == ProductionLineWipData.ProductionOrderId && x.ProductCode == ProductCode && x.IsReleased == true);
                if (ProductionOrderDetailsData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? " كود المنتج لا ينتمي لخط الإنتاج" : "Product code is not related to the production line");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, productionLineDetails, palletDetails };
                }

                decimal NoItemPerBox = 0;
                decimal NoBoxPerPallet = 0;
                decimal MaxReceivingQty = 0;

                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (PalletData != null)
                {

                    if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }
                    if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsChangedQuantityByWarehouse == true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم تغيير الكمية المستلمة للباليت من قبل مستخدم المستودع وتحتاج الى موافقة" : "The quantity received for the pallet has been changed by the warehouse user and needs approval");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }
                    if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.ProductCode == ProductCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? " كود المنتج لا ينتمي للباليت" : "Product code is not related to the pallet");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }
                    var productionOrderReceiving = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.BatchNo == ProductionOrderDetailsData.BatchNo  && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode);
                    NoItemPerBox = ((productionOrderReceiving.NumeratorforConversionPac ?? 0) / (productionOrderReceiving.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = ((productionOrderReceiving.NumeratorforConversionPal ?? 0) / (productionOrderReceiving.DenominatorforConversionPal ?? 0));
                    MaxReceivingQty = NoBoxPerPallet * NoItemPerBox;
                    if (MaxReceivingQty <= 0)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إضافة بسط المنتج والمقام للتحويل" : "Product numerator and denominator for conversion is not added");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }

                    if (DBContext.ProductionOrderReceivings.Any(x => x.PalletCode == PalletCode  && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode && MaxReceivingQty < (x.Qty + (CartoonReceivedQty * NoItemPerBox))))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكثر من سعة الباليت القصوى " : "You can't receive more than the maximum pallet capacity");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }

                    //if (ProductionOrderDetailsData.Qty < (ProductionOrderDetailsData.ReceivedQty + (CartoonReceivedQty * NoItemPerBox)))
                    //{
                    //    responseStatus.StatusCode = 401;
                    //    responseStatus.IsSuccess = false;
                    //    responseStatus.StatusMessage = ((applang == "ar") ? " الكميه المستلمه اكبر من كميه الباتش" : "Receiving quantity is greater than the released batch quantity");
                    //    return new { responseStatus, productionLineDetails, palletDetails };
                    //}

                    if (productionOrderReceiving != null)
                    {
                        productionOrderReceiving.Qty += (CartoonReceivedQty * (long)NoItemPerBox);
                        productionOrderReceiving.UserIdAdd = UserID;
                        productionOrderReceiving.DateTimeAdd = DateTime.Now;
                        productionOrderReceiving.DeviceSerialNoReceived = DeviceSerialNo;
                        productionOrderReceiving.NumeratorforConversionPac = ProductData.NumeratorforConversionPac;
                        productionOrderReceiving.NumeratorforConversionPal = ProductData.NumeratorforConversionPal;
                        productionOrderReceiving.DenominatorforConversionPac = ProductData.DenominatorforConversionPac;
                        productionOrderReceiving.DenominatorforConversionPal = ProductData.DenominatorforConversionPal;
                        productionOrderReceiving.IsExcessProductionReceiving = IsExcessProductionReceiving;

                    }
                    DBContext.ProductionOrderReceivings.Update(productionOrderReceiving);
                    PalletData.ReceivingQty += (CartoonReceivedQty * (long)NoItemPerBox);
                    DBContext.PalletWips.Update(PalletData);

                }
                else
                {
                    NoItemPerBox = ((ProductData.NumeratorforConversionPac ?? 0) / (ProductData.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = ((ProductData.NumeratorforConversionPal ?? 0) / (ProductData.DenominatorforConversionPal ?? 0));
                    MaxReceivingQty = NoBoxPerPallet * NoItemPerBox;
                    if (MaxReceivingQty <= 0)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إضافة بسط المنتج والمقام للتحويل" : "Product numerator and denominator for conversion is not added");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }
                    //if (ProductionOrderDetailsData.Qty < (ProductionOrderDetailsData.ReceivedQty + (CartoonReceivedQty * (long)NoItemPerBox)))
                    //{
                    //    responseStatus.StatusCode = 401;
                    //    responseStatus.IsSuccess = false;
                    //    responseStatus.StatusMessage = ((applang == "ar") ? " الكميه المستلمه اكبر من كميه الباتش" : "Receiving quantity is greater than the released batch quantity");
                    //    return new { responseStatus, productionLineDetails, palletDetails };
                    //}
                    if (MaxReceivingQty < (CartoonReceivedQty * (long)NoItemPerBox))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكثر من سعة الباليت القصوى" : "You can't receive more than the maximum pallet capacity");
                        return new { responseStatus, productionLineDetails, palletDetails };
                    }

                    PalletWip palletWip = new()
                    {
                        BatchNo = ProductionOrderDetailsData.BatchNo,
                        ProductCode = ProductionOrderDetailsData.ProductCode,
                        PalletCode = PalletCode,
                        PlantCode = ProductionOrderDetailsData.PlantCode,
                        ProductionLineId = ProductionOrderDetailsData.ProductionLineId,
                        ProductionOrderId = ProductionOrderDetailsData.ProductionOrderId,
                        SaporderId = ProductionOrderDetailsData.SapOrderId,
                        ReceivingQty = (CartoonReceivedQty * (long)NoItemPerBox),
                        IsWarehouseLocation = ProductData.IsWarehouseLocation,
                        ProductionDate = ProductionOrderDetailsData.ProductionDate
                    };
                    DBContext.PalletWips.Add(palletWip);

                    ProductionOrderReceiving productionOrderReceiving = new()
                    {
                        BatchNo = ProductionOrderDetailsData.BatchNo,
                        NumeratorforConversionPac = ProductData.NumeratorforConversionPac,
                        NumeratorforConversionPal = ProductData.NumeratorforConversionPal,
                        DenominatorforConversionPac = ProductData.DenominatorforConversionPac,
                        DenominatorforConversionPal = ProductData.DenominatorforConversionPal,
                        DateTimeAdd = DateTime.Now,
                        UserIdAdd = UserID,
                        ProductionDate = ProductionOrderDetailsData.ProductionDate,
                        DeviceSerialNoReceived = DeviceSerialNo,
                        IsWarehouseReceived = false,
                        ProductCode = ProductionOrderDetailsData.ProductCode,
                        PalletCode = PalletCode,
                        PlantCode = ProductionOrderDetailsData.PlantCode,
                        ProductionLineId = ProductionOrderDetailsData.ProductionLineId,
                        ProductionOrderId = ProductionOrderDetailsData.ProductionOrderId,
                        Qty = (CartoonReceivedQty * (long)NoItemPerBox),
                        SaporderId = ProductionOrderDetailsData.SapOrderId,
                        IsExcessProductionReceiving = IsExcessProductionReceiving
                    };
                    DBContext.ProductionOrderReceivings.Add(productionOrderReceiving);


                }
                var receivedQtyInUnits = ProductionOrderDetailsData.ReceivedQty + (CartoonReceivedQty * (long)NoItemPerBox);
                ProductionOrderDetailsData.ReceivedQty = receivedQtyInUnits;
                ProductionOrderDetailsData.BatchStatus = "Receiving";
                if (ProductionOrderDetailsData.ReceivedQty == ProductionOrderDetailsData.Qty)
                {
                    ProductionOrderDetailsData.IsReceived = true;
                    //DBContext.ProductionLineWips.Remove(ProductionLineWipData);
                }
                DBContext.ProductionOrderDetails.Update(ProductionOrderDetailsData);
                DBContext.SaveChanges();


                

                palletDetails = (from b in DBContext.PalletWips
                                     join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                     join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                     join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                     join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                     join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                     join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                     where b.PalletCode == PalletCode
                                     select new PalletDetailsParam
                                     {
                                         PlantId = x.PlantId,
                                         PlantCode = x.PlantCode,
                                         PlantDesc = x.PlantDesc,
                                         OrderTypeId = o.OrderTypeId,
                                         OrderTypeCode = o.OrderTypeCode,
                                         OrderTypeDesc = o.OrderTypeDesc,
                                         ProductId = p.ProductId,
                                         ProductCode = p.ProductCode,
                                         ProductDesc = p.ProductDesc,
                                         NumeratorforConversionPac = p.NumeratorforConversionPac,
                                         NumeratorforConversionPal = p.NumeratorforConversionPal,
                                         DenominatorforConversionPal = p.DenominatorforConversionPal,
                                         DenominatorforConversionPac = p.DenominatorforConversionPac,
                                         ProductionOrderId = l.ProductionOrderId,
                                         SapOrderId = l.SapOrderId.Value,
                                         ProductionOrderQty = l.Qty,
                                         ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                         OrderDate = l.OrderDate,
                                         BatchNo = r.BatchNo,
                                         PalletQty = r.Qty,
                                         IsWarehouseLocation = b.IsWarehouseLocation,
                                         ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                         ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                         IsWarehouseReceived = b.IsWarehouseReceived,
                                         StorageLocationCode = b.StorageLocationCode,
                                         WarehouseReceivingQty = b.WarehouseReceivingQty,
                                         LaneCode = b.LaneCode
                                     }).FirstOrDefault();

                //productionLineDetails = (from b in DBContext.ProductionOrderReceivings
                //                         join d in DBContext.ProductionOrderDetails on new
                //                         {
                //                             ProductionOrderId = b.ProductionOrderId.Value,
                //                             BatchNo = b.BatchNo,
                //                             ProductionLineId = b.ProductionLineId,
                //                             ProductCode = b.ProductCode
                //                         } equals new
                //                         {
                //                             ProductionOrderId = d.ProductionOrderId,
                //                             BatchNo = d.BatchNo,
                //                             ProductionLineId = d.ProductionLineId,
                //                             ProductCode = d.ProductCode
                //                         }
                //                         join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                //                         join p in DBContext.Products on l.ProductCode equals p.ProductCode
                //                         join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                //                         join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                //                         join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                //                         where ln.ProductionLineCode == ProductionLineCode && ln.PlantCode == PlantCode
                //                         select new ProductionLineDetailsParam
                //                         {
                //                             PlantId = x.PlantId,
                //                             PlantCode = x.PlantCode,
                //                             PlantDesc = x.PlantDesc,
                //                             OrderTypeId = o.OrderTypeId,
                //                             OrderTypeCode = o.OrderTypeCode,
                //                             OrderTypeDesc = o.OrderTypeDesc,
                //                             ProductId = p.ProductId,
                //                             ProductCode = p.ProductCode,
                //                             ProductDesc = p.ProductDesc,
                //                             ProductionOrderId = l.ProductionOrderId,
                //                             SapOrderId = l.SapOrderId.Value,
                //                             ProductionOrderQty = l.Qty,
                //                             OrderDate = l.OrderDate,
                //                             OrderDetailsId = d.OrderDetailsId,
                //                             BatchNo = d.BatchNo,
                //                             BatchQty = d.Qty,
                //                             ProductionDate = d.ProductionDate,
                //                             IsClosedBatch = d.IsClosedBatch,
                //                             IsReleased = d.IsReleased,
                //                             BatchStatus = d.BatchStatus,
                //                             IsReceived = d.IsReceived,
                //                             ReceivedQty = d.ReceivedQty,
                //                             IsWarehouseLocation = p.IsWarehouseLocation,
                //                             ProductionLineId = ln.ProductionLineId,
                //                             ProductionLineCode = ln.ProductionLineCode,
                //                             ProductionLineDesc = ln.ProductionLineDesc
                //                         }).FirstOrDefault();

                var GetProductionLineDetails = (from b in DBContext.ProductionLineWips
                                                join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                                join r in DBContext.ProductionOrderReceivings on new { b.ProductionOrderId, b.BatchNo } equals new { ProductionOrderId = r.ProductionOrderId.Value, r.BatchNo } into rr
                                                from r in rr.DefaultIfEmpty()
                                                join d in DBContext.ProductionOrderDetails on b.OrderDetailsId equals d.OrderDetailsId
                                                join p in DBContext.Products on l.ProductCode equals p.ProductCode
                                                join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                                join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                                join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                                where ln.ProductionLineId == ProductionOrderDetailsData.ProductionLineId && ln.PlantCode == PlantCode
                                                select new ProductionLineDetailsParam
                                                {
                                                    PlantId = x.PlantId,
                                                    PlantCode = x.PlantCode,
                                                    PlantDesc = x.PlantDesc,
                                                    OrderTypeId = o.OrderTypeId,
                                                    OrderTypeCode = o.OrderTypeCode,
                                                    OrderTypeDesc = o.OrderTypeDesc,
                                                    ProductId = p.ProductId,
                                                    ProductCode = p.ProductCode,
                                                    ProductDesc = p.ProductDesc,
                                                    ProductionOrderId = b.ProductionOrderId,
                                                    SapOrderId = b.SapOrderId,
                                                    ProductionOrderQty = l.Qty,
                                                    ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    OrderDate = l.OrderDate,
                                                    OrderDetailsId = b.OrderDetailsId,
                                                    BatchNo = d.BatchNo,
                                                    BatchQty = d.Qty,
                                                    BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    NoCanPerCartoon = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0)) : (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0)),
                                                    NoCartoonPerPallet = (r.BatchNo != null) ? (long)((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) : (long)((p.NumeratorforConversionPal ?? 0) / (p.DenominatorforConversionPal ?? 0)),
                                                    ProductionDate = d.ProductionDate,
                                                    IsClosedBatch = d.IsClosedBatch,
                                                    IsReleased = d.IsReleased,
                                                    BatchStatus = d.BatchStatus,
                                                    IsReceived = d.IsReceived,
                                                    ReceivedQty = d.ReceivedQty,
                                                    ReceivedQtyCartoon = (r.BatchNo != null) ? (d.ReceivedQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.ReceivedQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                                    ProductionLineId = ln.ProductionLineId,
                                                    ProductionLineCode = ln.ProductionLineCode,
                                                    ProductionLineDesc = ln.ProductionLineDesc
                                                }).Distinct().ToList();
                

                productionLineDetails = (from b in GetProductionLineDetails
                                         select new ProductionLineDetailsParam
                                         {
                                             PlantId = b.PlantId,
                                             PlantCode = b.PlantCode,
                                             PlantDesc = b.PlantDesc,
                                             OrderTypeId = b.OrderTypeId,
                                             OrderTypeCode = b.OrderTypeCode,
                                             OrderTypeDesc = b.OrderTypeDesc,
                                             ProductId = b.ProductId,
                                             ProductCode = b.ProductCode,
                                             ProductDesc = b.ProductDesc,
                                             ProductionOrderId = b.ProductionOrderId,
                                             SapOrderId = b.SapOrderId,
                                             ProductionOrderQty = b.ProductionOrderQty,
                                             ProductionOrderQtyCartoon = b.ProductionOrderQtyCartoon,
                                             OrderDate = b.OrderDate,
                                             OrderDetailsId = b.OrderDetailsId,
                                             BatchNo = b.BatchNo,
                                             BatchQty = b.BatchQty,
                                             BatchQtyCartoon = b.BatchQtyCartoon,
                                             NoCanPerCartoon = b.NoCanPerCartoon,
                                             NoCartoonPerPallet = b.NoCartoonPerPallet,
                                             PalletCapacity = (b.NoCartoonPerPallet * b.NoCanPerCartoon),
                                             ProductionDate = b.ProductionDate,
                                             IsClosedBatch = b.IsClosedBatch,
                                             IsReleased = b.IsReleased,
                                             BatchStatus = b.BatchStatus,
                                             IsReceived = b.IsReceived,
                                             ReceivedQty = b.ReceivedQty,
                                             ReceivedQtyCartoon = b.ReceivedQtyCartoon,
                                             ProductionLineId = b.ProductionLineId,
                                             ProductionLineCode = b.ProductionLineCode,
                                             ProductionLineDesc = b.ProductionLineDesc
                                         }).FirstOrDefault();

                if (productionLineDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, productionLineDetails, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object CancelPallet(long UserID, string DeviceSerialNo, string applang, string PalletCode, string CancelReason)
        {
            ResponseStatus responseStatus = new();
            try
            {

                if (CancelReason == null || string.IsNullOrEmpty(CancelReason.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال سبب الرفض" : "Missing cancel reason");
                    return new { responseStatus };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus };
                }
                decimal NoItemPerBox = 0;
                decimal NoBoxPerPallet = 0;
                decimal MaxReceivingQty = 0;

                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (PalletData != null)
                {
                    var ProductionOrderDetailsData = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.IsReleased == true);
                    if (ProductionOrderDetailsData == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? " كود المنتج لا ينتمي لخط الإنتاج" : "Product code is not related to the production line");
                        return new { responseStatus, };
                    }

                    if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                        return new { responseStatus };
                    }

                    var productionOrderReceiving = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.ProductionLineId == PalletData.ProductionLineId && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode);
                    NoItemPerBox = ((productionOrderReceiving.NumeratorforConversionPac ?? 0) / (productionOrderReceiving.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = ((productionOrderReceiving.NumeratorforConversionPal ?? 0) / (productionOrderReceiving.DenominatorforConversionPal ?? 0));
                    MaxReceivingQty = NoBoxPerPallet * NoItemPerBox;
                    if (productionOrderReceiving != null)
                    {
                        ProductionOrderDetailsData.ReceivedQty -= PalletData.ReceivingQty;
                        ProductionOrderDetailsData.IsReceived = false;
                        DBContext.ProductionOrderDetails.Update(ProductionOrderDetailsData);

                        DBContext.ProductionOrderReceivings.Remove(productionOrderReceiving);
                        DBContext.PalletWips.Remove(PalletData);
                    }
                    else
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت من قبل" : "The pallet was not located in the WIP");
                        return new { responseStatus };
                    }

                    CancelPallet cancelPallet = new CancelPallet()
                    {
                        PalletCode = PalletCode,
                        Reason = CancelReason,
                        UserId = UserID
                    };

                    DBContext.CancelPallets.Add(cancelPallet);
                }
                else
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت من قبل" : "The pallet was not located in the WIP");
                    return new { responseStatus };
                }


                DBContext.SaveChanges();

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object ChangeQtyPallet(long UserID, string DeviceSerialNo, string applang, string PalletCode, long? CartoonReceivedQty, string ChangeQtyReason)
        {
            ResponseStatus responseStatus = new();
            try
            {

                if (ChangeQtyReason == null || string.IsNullOrEmpty(ChangeQtyReason.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال السبب " : "Missing reason");
                    return new { responseStatus };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus };
                }
                decimal NoItemPerBox = 0;
                decimal NoBoxPerPallet = 0;
                long OldReceivingQty = 0;
                long NewReceivingQty = 0;

                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (PalletData != null)
                {
                    var ProductionOrderDetailsData = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.IsReleased == true);
                    if (ProductionOrderDetailsData == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? " كود المنتج لا ينتمي لخط الإنتاج" : "Product code is not related to the production line");
                        return new { responseStatus, };
                    }

                    if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                        return new { responseStatus };
                    }

                    var productionOrderReceiving = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.ProductionLineId == PalletData.ProductionLineId && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode);
                    if (productionOrderReceiving != null)
                    {
                        NoItemPerBox = ((productionOrderReceiving.NumeratorforConversionPac ?? 0) / (productionOrderReceiving.DenominatorforConversionPac ?? 0));
                        NoBoxPerPallet = ((productionOrderReceiving.NumeratorforConversionPal ?? 0) / (productionOrderReceiving.DenominatorforConversionPal ?? 0));
                        NewReceivingQty = ((CartoonReceivedQty ?? 0) * (long)NoItemPerBox);
                        OldReceivingQty = PalletData.ReceivingQty ?? 0;


                        ProductionOrderDetailsData.ReceivedQty = NewReceivingQty;
                        DBContext.ProductionOrderDetails.Update(ProductionOrderDetailsData);

                        productionOrderReceiving.Qty = NewReceivingQty;

                        DBContext.ProductionOrderReceivings.Update(productionOrderReceiving);
                        PalletData.ReceivingQty = NewReceivingQty;

                        DBContext.PalletWips.Update(PalletData);
                    }
                    else
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت من قبل" : "The pallet was not located in the WIP");
                        return new { responseStatus };
                    }

                    ChangeQtyPallet changeQtyPallet = new ChangeQtyPallet()
                    {
                        PalletCode = PalletCode,
                        Reason = ChangeQtyReason,
                        UserId = UserID,
                        Qty = OldReceivingQty,
                        NewQty = NewReceivingQty
                    };

                    DBContext.ChangeQtyPallets.Add(changeQtyPallet);
                    
                }
                else
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت من قبل" : "The pallet was not located in the WIP");
                    return new { responseStatus };
                }


                DBContext.SaveChanges();

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpPost]
        public object PalletTransfer([FromBody] PalletTransferParam model)
        {
            ResponseStatus responseStatus = new();
            //PalletDetailsParam palletDetails = new();
            //PalletTransferTransaction palletTransferTransaction = new();
            try
            {
                ProductionOrder productionOrder = DBContext.ProductionOrders.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId);
                if (productionOrder == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "معرف أمر الشغل خاطئ" : "Production Order Id is wrong");
                    return new { responseStatus };
                }
                if (model.BatchNo == null || string.IsNullOrEmpty(model.BatchNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
                    return new { responseStatus };
                }
                ProductionOrderDetail productionOrderDetail = DBContext.ProductionOrderDetails.FirstOrDefault(x => x.ProductionOrderId == model.ProductionOrderId && x.BatchNo == model.BatchNo);
                if (productionOrderDetail == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "رقم الباتش لا ينتمي لأمر الشغل التي تم اختياره" : "Batch no is not related to the specified production Order");
                    return new { responseStatus };
                }
                if (productionOrderDetail.IsReleased != true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم تحرير رقم الباتش من قبل" : "Batch no is not released yet");
                    return new { responseStatus };
                }
                if (model.PalletsCode == null || model.PalletsCode.Count < 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus };
                }
                foreach (var palletCode in model.PalletsCode)
                {
                    if (!DBContext.Pallets.Any(x => x.PalletCode == palletCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                        return new { responseStatus };
                    }

                    PalletWip PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == palletCode);
                    if (PalletData == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                        return new { responseStatus };
                    }
                    if (PalletData.IsWarehouseReceived == true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لا يمكن تحويل باليت تم استلامها بالفعل في المستودع" : "Can not transfer a received pallet in the warehouse");
                        return new { responseStatus };
                    }
                    if (productionOrderDetail.ProductCode != PalletData.ProductCode)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? " كود المنتج الموجود على الباليت لا ينتمي للباتش المختاره" : "Pallet product code is not related to the selected batch");
                        return new { responseStatus };
                    }
                }

                foreach (var palletCode in model.PalletsCode)
                {
                    PalletWip PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == palletCode);
                    ProductionOrderReceiving productionOrderReceiving = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == palletCode);
                    if (productionOrderReceiving != null)
                    {
                        PalletTransferTransaction palletTransferTransaction_add = new()
                        {
                            BatchNoFrom = productionOrderReceiving.BatchNo,
                            DateTimeReceived = productionOrderReceiving.DateTimeAdd,
                            UserIdReceived = productionOrderReceiving.UserIdAdd,
                            ProductionDateFrom = productionOrderReceiving.ProductionDate,
                            DeviceSerialNoReceived = productionOrderReceiving.DeviceSerialNoReceived,
                            PlantCodeFrom = productionOrderReceiving.PlantCode,
                            ProductionLineIdFrom = productionOrderReceiving.ProductionLineId,
                            ProductionOrderIdFrom = productionOrderReceiving.ProductionOrderId,
                            SaporderIdFrom = productionOrderReceiving.SaporderId,

                            BatchNoTo = productionOrderDetail.BatchNo,
                            DateTimeAdd = DateTime.Now,
                            UserIdAdd = model.UserID,
                            ProductionDateTo = productionOrderDetail.ProductionDate,
                            DeviceSerialNoTransfered = model.DeviceSerialNo,
                            PlantCodeTo = productionOrderDetail.PlantCode,
                            ProductionLineIdTo = productionOrderDetail.ProductionLineId,
                            ProductionOrderIdTo = productionOrderDetail.ProductionOrderId,
                            SaporderIdTo = productionOrderDetail.SapOrderId,

                            ProductCode = productionOrderReceiving.ProductCode,
                            PalletCode = palletCode,
                            Qty = productionOrderReceiving.Qty,

                            NumeratorforConversionPac = productionOrderReceiving.NumeratorforConversionPac,
                            NumeratorforConversionPal = productionOrderReceiving.NumeratorforConversionPal,
                            DenominatorforConversionPac = productionOrderReceiving.DenominatorforConversionPac,
                            DenominatorforConversionPal = productionOrderReceiving.DenominatorforConversionPal
                        };
                        DBContext.PalletTransferTransactions.Add(palletTransferTransaction_add);

                        PalletData.BatchNo = productionOrderDetail.BatchNo;
                        PalletData.PlantCode = productionOrderDetail.PlantCode;
                        PalletData.ProductionLineId = productionOrderDetail.ProductionLineId;
                        PalletData.ProductionOrderId = productionOrderDetail.ProductionOrderId;
                        PalletData.SaporderId = productionOrderDetail.SapOrderId;
                        PalletData.ProductionDate = productionOrderDetail.ProductionDate;
                        DBContext.PalletWips.Update(PalletData);

                        productionOrderReceiving.BatchNo = productionOrderDetail.BatchNo;
                        productionOrderReceiving.DateTimeAdd = DateTime.Now;
                        productionOrderReceiving.UserIdAdd = model.UserID;
                        productionOrderReceiving.ProductionDate = productionOrderDetail.ProductionDate;
                        productionOrderReceiving.DeviceSerialNoReceived = model.DeviceSerialNo;
                        productionOrderReceiving.PlantCode = productionOrderDetail.PlantCode;
                        productionOrderReceiving.ProductionLineId = productionOrderDetail.ProductionLineId;
                        productionOrderReceiving.ProductionOrderId = productionOrderDetail.ProductionOrderId;
                        productionOrderReceiving.SaporderId = productionOrderDetail.SapOrderId;

                        DBContext.ProductionOrderReceivings.Update(productionOrderReceiving);
                        DBContext.SaveChanges();
                    }
                }

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");

                //palletDetails = (from b in DBContext.PalletWips
                //                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                //                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                //                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                //                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                //                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                //                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                //                 where b.PalletCode == PalletCode
                //                 select new PalletDetailsParam
                //                 {
                //                     PlantId = x.PlantId,
                //                     PlantCode = x.PlantCode,
                //                     PlantDesc = x.PlantDesc,
                //                     OrderTypeId = o.OrderTypeId,
                //                     OrderTypeCode = o.OrderTypeCode,
                //                     OrderTypeDesc = o.OrderTypeDesc,
                //                     ProductId = p.ProductId,
                //                     ProductCode = p.ProductCode,
                //                     ProductDesc = p.ProductDesc,
                //                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                //                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                //                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                //                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                //                     ProductionOrderId = l.ProductionOrderId,
                //                     SapOrderId = l.SapOrderId.Value,
                //                     ProductionOrderQty = l.Qty,
                //                     OrderDate = l.OrderDate,
                //                     BatchNo = r.BatchNo,
                //                     PalletQty = r.Qty,
                //                     IsWarehouseLocation = b.IsWarehouseLocation,
                //                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                //                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                //                     IsWarehouseReceived = b.IsWarehouseReceived,
                //                     StorageLocationCode = b.StorageLocationCode,
                //     //}                WarehouseReceivingQty = b.WarehouseReceivingQty,
                //                     LaneCode = b.LaneCode
                //                 }).FirstOrDefault();
                //palletTransferTransaction = DBContext.PalletTransferTransactions.FirstOrDefault(x => x.PalletCode == PalletCode && x.BatchNoTo == BatchNo && x.ProductionOrderIdTo == ProductionOrderId);


                //if (palletDetails != null)
                //{
                //    responseStatus.StatusCode = 200;
                //    responseStatus.IsSuccess = true;

                //    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                //}
                //else
                //{
                //    responseStatus.StatusCode = 400;
                //    responseStatus.IsSuccess = false;

                //    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل خط الإنتاج" : "Production line not loaded");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            //return new { responseStatus, palletDetails, palletTransferTransaction };
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpPost]
        public object PalletTransferToPallet([FromBody] PalletTransferToPalletParam model)
        {
            ResponseStatus responseStatus = new();
            try
            {
                decimal NoItemPerBox = 0;
                decimal NoBoxPerPallet = 0;
                long QtyToTransfer = 0;
                if (model.CartoonReceivedQty == null || string.IsNullOrEmpty(model.CartoonReceivedQty.ToString().Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الكمية " : "Missing Cartoon Qty");
                    return new { responseStatus, };
                }
                if (model.PalletCodeFrom == null || string.IsNullOrEmpty(model.PalletCodeFrom))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود الباليت من" : "Missing pallet code from");
                    return new { responseStatus };
                }

                if (!DBContext.Pallets.Any(x => x.PalletCode == model.PalletCodeFrom))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود الباليت الاول خاطئ" : "Pallet code (from) is wrong");
                    return new { responseStatus };
                }
                if (model.PalletCodeTo == null || string.IsNullOrEmpty(model.PalletCodeTo))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود الباليت الي" : "Missing pallet code to");
                    return new { responseStatus };
                }

                if (!DBContext.Pallets.Any(x => x.PalletCode == model.PalletCodeTo))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود الباليت الثامي خاطئ" : "Pallet code (To) is wrong");
                    return new { responseStatus };
                }

                PalletWip PalletDataFrom = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == model.PalletCodeFrom);
                if (PalletDataFrom == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم تحميل الباليت الاولي" : "Pallet (From) is not loaded");
                    return new { responseStatus };
                }

                if (PalletDataFrom.IsWarehouseReceived == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لا يمكن تحويل باليت تم استلامها بالفعل في المستودع" : "Can not transfer a received pallet in the warehouse");
                    return new { responseStatus };
                }
                PalletWip PalletDataTo = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == model.PalletCodeTo);
                if (PalletDataTo != null)
                {
                    if (PalletDataTo.IsWarehouseReceived == true)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "لا يمكن تحويل باليت تم استلامها بالفعل في المستودع" : "Can not transfer a received pallet in the warehouse");
                        return new { responseStatus };
                    }

                    if (PalletDataTo.ProductCode != PalletDataFrom.ProductCode)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? " كود المنتج الموجود على الباليتات مختلف" : "Pallets product code is not same");
                        return new { responseStatus };
                    }
                }
                ProductionOrderReceiving productionOrderReceivingFrom = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == model.PalletCodeFrom);
                if (productionOrderReceivingFrom != null)
                {
                    NoItemPerBox = ((productionOrderReceivingFrom.NumeratorforConversionPac ?? 0) / (productionOrderReceivingFrom.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = ((productionOrderReceivingFrom.NumeratorforConversionPal ?? 0) / (productionOrderReceivingFrom.DenominatorforConversionPal ?? 0));
                    long OldReceivingQty = PalletDataFrom.ReceivingQty ?? 0;
                    QtyToTransfer = ((model.CartoonReceivedQty ?? 0) * (long)NoItemPerBox);
                    if (QtyToTransfer > OldReceivingQty)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? " لا يمكن نقل كميه اكبر من كميه الباليت" : "Can not transfer quantity greater than the pallet quantity");
                        return new { responseStatus };
                    }

                    if (PalletDataTo != null)
                    {
                        ProductionOrderReceiving productionOrderReceivingTo = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == model.PalletCodeTo);
                        if (productionOrderReceivingTo != null)
                        {
                            PalletTransferToPalletTransaction palletTransferTransaction_add = new()
                            {
                                DateTimeAdd = DateTime.Now,
                                UserIdAdd = model.UserID,
                                PalletCodeFrom = model.PalletCodeFrom,
                                PalletCodeTo = model.PalletCodeTo,
                                Qty = QtyToTransfer
                            };
                            DBContext.PalletTransferToPalletTransactions.Add(palletTransferTransaction_add);


                            PalletDataFrom.ReceivingQty -= QtyToTransfer;

                            productionOrderReceivingFrom.Qty -= QtyToTransfer;

                            PalletDataTo.ReceivingQty += QtyToTransfer;
                            DBContext.PalletWips.Update(PalletDataTo);

                            productionOrderReceivingTo.Qty += QtyToTransfer;
                            DBContext.ProductionOrderReceivings.Update(productionOrderReceivingTo);


                            if (productionOrderReceivingFrom.Qty > 0) DBContext.ProductionOrderReceivings.Update(productionOrderReceivingFrom);
                            else DBContext.ProductionOrderReceivings.Remove(productionOrderReceivingFrom);

                            if (PalletDataFrom.ReceivingQty > 0) DBContext.PalletWips.Update(PalletDataFrom);
                            else DBContext.PalletWips.Remove(PalletDataFrom);

                            DBContext.SaveChanges();
                        }
                    }
                    else
                    {
                        PalletTransferToPalletTransaction palletTransferTransaction_add = new()
                        {
                            DateTimeAdd = DateTime.Now,
                            UserIdAdd = model.UserID,
                            PalletCodeFrom = model.PalletCodeFrom,
                            PalletCodeTo = model.PalletCodeTo,
                            Qty = QtyToTransfer
                        };
                        DBContext.PalletTransferToPalletTransactions.Add(palletTransferTransaction_add);


                        PalletDataFrom.ReceivingQty -= QtyToTransfer;
                        productionOrderReceivingFrom.Qty -= QtyToTransfer;

                        PalletWip PalletDataTo_Add = new PalletWip()
                        {
                            ReceivingQty = QtyToTransfer,
                            BatchNo = PalletDataFrom.BatchNo,
                            PalletCode = model.PalletCodeTo,
                            DateTimePutAway = PalletDataFrom.DateTimePutAway,
                            DateTimeWarehouse = PalletDataFrom.DateTimeWarehouse,
                            DeviceSerialNoCheckIn = PalletDataFrom.DeviceSerialNoCheckIn,
                            DeviceSerialNoCheckOut = PalletDataFrom.DeviceSerialNoCheckOut,
                            DeviceSerialNoPutAway = PalletDataFrom.DeviceSerialNoPutAway,
                            DeviceSerialNoWarehouse = PalletDataFrom.DeviceSerialNoWarehouse,
                            IsChangedQuantityByWarehouse = PalletDataFrom.IsChangedQuantityByWarehouse,
                            IsPickup = PalletDataFrom.IsPickup,
                            IsProductionTakeAction = PalletDataFrom.IsProductionTakeAction,
                            IsWarehouseLocation = PalletDataFrom.IsWarehouseLocation,
                            IsWarehouseReceived = PalletDataFrom.IsWarehouseReceived,
                            LaneCode = PalletDataFrom.LaneCode,
                            PlantCode = PalletDataFrom.PlantCode,
                            ProductCode = PalletDataFrom.ProductCode,
                            ProductionDate = PalletDataFrom.ProductionDate,
                            ProductionLineId = PalletDataFrom.ProductionLineId,
                            ProductionOrderId = PalletDataFrom.ProductionOrderId,
                            SaporderId = PalletDataFrom.SaporderId,
                            StorageLocationCode = PalletDataFrom.StorageLocationCode,
                            WarehouseReceivingQty = 0,
                        };

                        DBContext.PalletWips.Add(PalletDataTo_Add);

                        ProductionOrderReceiving productionOrderReceiving_Add = new ProductionOrderReceiving()
                        {
                            Qty = QtyToTransfer,
                            BatchNo = productionOrderReceivingFrom.BatchNo,
                            PalletCode = model.PalletCodeTo,
                            DenominatorforConversionPac = productionOrderReceivingFrom.DenominatorforConversionPac,
                            DenominatorforConversionPal = productionOrderReceivingFrom.DenominatorforConversionPal,
                            StorageLocationCode = productionOrderReceivingFrom.StorageLocationCode,
                            SaporderId = productionOrderReceivingFrom.SaporderId,
                            ProductionOrderId = productionOrderReceivingFrom.ProductionOrderId,
                            ProductionLineId = productionOrderReceivingFrom.ProductionLineId,
                            ProductionDate = productionOrderReceivingFrom.ProductionDate,
                            ProductCode = productionOrderReceivingFrom.ProductCode,
                            PlantCode = productionOrderReceivingFrom.PlantCode,
                            UserIdAdd = model.UserID,
                            DateTimeAdd = DateTime.Now,
                            DeviceSerialNoReceived = model.DeviceSerialNo,
                            NumeratorforConversionPac = productionOrderReceivingFrom.NumeratorforConversionPac,
                            NumeratorforConversionPal = productionOrderReceivingFrom.NumeratorforConversionPal
                        };

                        DBContext.ProductionOrderReceivings.Add(productionOrderReceiving_Add);


                        if (productionOrderReceivingFrom.Qty > 0) DBContext.ProductionOrderReceivings.Update(productionOrderReceivingFrom);
                        else DBContext.ProductionOrderReceivings.Remove(productionOrderReceivingFrom);

                        if (PalletDataFrom.ReceivingQty > 0) DBContext.PalletWips.Update(PalletDataFrom);
                        else DBContext.PalletWips.Remove(PalletDataFrom);

                        DBContext.SaveChanges();
                    }


                }
                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetReceivedList(long UserID, string DeviceSerialNo, string applang, string ProductionLineCode)
        {
            ResponseStatus responseStatus = new();
            List<ReceivedPalletDetailsParam> GetData = new();
            try
            {

                if (ProductionLineCode == null || string.IsNullOrEmpty(ProductionLineCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود خط الانتاج" : "Missing production line code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.ProductionLines.Any(x => x.ProductionLineCode == ProductionLineCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود خط الانتاج خاطئ" : "Production line code is wrong");
                    return new { responseStatus, GetData };
                }


                GetData = (from b in DBContext.PalletWips
                           join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                           join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                           join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                           where ln.ProductionLineCode == ProductionLineCode
                           select new ReceivedPalletDetailsParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               ProductionOrderId = l.ProductionOrderId,
                               BatchNo = r.BatchNo,
                               PalletQty = r.Qty,
                               PalletCartoonQty = (r.Qty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               PalletCode = r.PalletCode,
                               ProductionLineId = ln.ProductionLineId,
                               ProductionLineCode = ln.ProductionLineCode,
                               ProductionLineDesc = ln.ProductionLineDesc,
                               IsChangedQuantityByWarehouse = b.IsChangedQuantityByWarehouse,
                               IsProductionTakeAction = b.IsProductionTakeAction
                           }).ToList();

                if (GetData != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPallets_ChangedQtyByWarehouseAndNeedProductionApproval_List(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ReceivedPalletDetailsNeedApprovalParam> GetData = new();
            try
            {


                GetData = (from b in DBContext.ReceivingPalletsNeedApprovals
                           join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                           join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                           join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                           where b.IsProductionApproved == null
                           select new ReceivedPalletDetailsNeedApprovalParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               ProductionOrderId = l.ProductionOrderId,
                               BatchNo = r.BatchNo,
                               WarehouseReceivedCartoonQty = b.WarehouseCartoonReceivingQty,
                               WarehouseReceivedQty = b.WarehouseReceivingQty,
                               PalletCode = r.PalletCode,
                               ProductionLineId = ln.ProductionLineId,
                               ProductionLineCode = ln.ProductionLineCode,
                               ProductionLineDesc = ln.ProductionLineDesc,
                               ProductionDate = r.ProductionDate,
                               ProductionReceivedCartonQty = (r.Qty.Value / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))),
                           }).ToList();

                if (GetData != null && GetData.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object Pallets_ChangedQtyByWarehouse_ProductionApprovalTakeAction(long UserID, string DeviceSerialNo, string applang, string PalletCode, string Comment, bool? IsProductionApproved)
        {
            ResponseStatus responseStatus = new();
            try
            {

                if (IsProductionApproved == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال IsProductionApproved" : "Missing IsProductionApproved");
                    return new { responseStatus };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                    return new { responseStatus };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsChangedQuantityByWarehouse != true))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تغيير كمية الباليت بواسطة مستخدم المستودع لذالك لا تحتاج الى موافقة" : "The pallet quantity has not been changed by the warehouse user. Therefore it does not reqire approval");
                    return new { responseStatus };
                }
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);

                if (DBContext.ReceivingPalletsNeedApprovals.Any(x => x.PalletCode == PalletCode && x.IsProductionApproved != null && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم اتخاذ قرار الموافقة من قبل" : "The approval decision has already been made before");
                    return new { responseStatus };
                }

                if (PalletData != null)
                {
                    PalletData.IsProductionTakeAction = true;
                    DBContext.PalletWips.Update(PalletData);
                    DBContext.SaveChanges();
                }
                ReceivingPalletsNeedApproval receivingPalletsNeedApproval = DBContext.ReceivingPalletsNeedApprovals.FirstOrDefault(x => x.PalletCode == PalletCode && x.IsProductionApproved == null);
                if (receivingPalletsNeedApproval != null)
                {
                    receivingPalletsNeedApproval.IsProductionApproved = IsProductionApproved;
                    receivingPalletsNeedApproval.ProductionComment = Comment;
                    receivingPalletsNeedApproval.UserIdProductionApproved = UserID;
                    receivingPalletsNeedApproval.DateTimeProductionApproved = DateTime.Now;
                    receivingPalletsNeedApproval.DeviceSerialNoProductionApproved = DeviceSerialNo;
                    DBContext.ReceivingPalletsNeedApprovals.Update(receivingPalletsNeedApproval);
                    DBContext.SaveChanges();

                    PalletData.ReceivingQty = receivingPalletsNeedApproval.WarehouseReceivingQty;
                    DBContext.PalletWips.Update(PalletData);
                    DBContext.SaveChanges();
                }

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");


            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetReceivedPalletsInWarehouse(long UserID, string DeviceSerialNo, string applang, DateTime? ProductionDate, string BatchNo,string ProductCode, DateTime? ReceivingDate)
        {
            ResponseStatus responseStatus = new();
            List<PalletDetailsParam> palletDetails = new();
            try
            {
                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join d in DBContext.ProductionOrderDetails on new
                                 {
                                     ProductionOrderId = b.ProductionOrderId.Value,
                                     BatchNo = b.BatchNo,
                                     ProductionLineId = b.ProductionLineId.Value
                                 } equals new
                                 {
                                     d.ProductionOrderId,
                                     d.BatchNo,
                                     ProductionLineId = d.ProductionLineId.Value
                                 }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.IsWarehouseReceived == true
                                  && (ProductionDate == null || b.ProductionDate.Value.Date == ProductionDate.Value.Date)
                                  && (BatchNo == null || b.BatchNo == BatchNo)
                                  && (ReceivingDate == null || b.DateTimeWarehouse.Value.Date == ReceivingDate.Value.Date)
                                  && (ProductCode == null || b.ProductCode.Contains(ProductCode))
                                 select new PalletDetailsParam
                                 {
                                     ReceivingDate = b.DateTimeWarehouse,
                                     PalletCode = b.PalletCode,
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletQty = r.Qty,
                                     PalletCartoonQty = Math.Round((r.Qty / ((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))).Value, 2),
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     WarehouseReceivingCartoonQty = ((b.WarehouseReceivingQty > 0) ? Math.Round((b.WarehouseReceivingQty / ((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))).Value, 2) : 0),
                                     LaneCode = b.LaneCode,
                                     AvailableQty = (long)(((r.NumeratorforConversionPal ?? 0) / (r.DenominatorforConversionPal ?? 0)) - Math.Round((r.Qty / ((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))).Value, 2)),
                                     ProductionLineId = ln.ProductionLineId,
                                     ProductionLineCode = ln.ProductionLineCode,
                                     ProductionLineDesc = ln.ProductionLineDesc,
                                     BatchQty = d.Qty,
                                     BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     ProductionDate = b.ProductionDate,
                                     ReceivedQty = b.ReceivingQty,
                                     ReceivedQtyCartoon = (r.BatchNo != null) ? (b.ReceivingQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (b.ReceivingQty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                 }).Distinct().ToList();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object WarehouseLocation_PalletCheckIn(long UserID, string DeviceSerialNo, string applang, string PalletCode, long? ProductionQty)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            try
            {

                if (ProductionQty == null || string.IsNullOrEmpty(ProductionQty.ToString().Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية الانتاج" : "Missing Production Qty");
                    return new { responseStatus, palletDetails };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus, palletDetails };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                    return new { responseStatus, palletDetails };
                }
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (PalletData != null)
                {
                    if (DBContext.Products.Any(x => x.ProductCode == PalletData.ProductCode && x.IsWarehouseLocation != true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لا يحتاج المنتج الموجود على الباليت المحددة إلى التخزين في موقع المخزن" : "Product that was in the specified pallet is not need to store in warehouse location");
                        return new { responseStatus, palletDetails };
                    }
                    PalletData.ProductionQtyCheckIn = ProductionQty;
                    PalletData.DeviceSerialNoCheckIn = DeviceSerialNo;
                    DBContext.PalletWips.Update(PalletData);
                }
                var productionOrderReceivings = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductionLineId == PalletData.ProductionLineId);
                if (productionOrderReceivings != null)
                {
                    productionOrderReceivings.ProductionQtyCheckIn = ProductionQty;
                    productionOrderReceivings.UserIdCheckIn = UserID;
                    productionOrderReceivings.DateTimeCheckIn = DateTime.Now;
                    productionOrderReceivings.DeviceSerialNoCheckIn = DeviceSerialNo;
                    DBContext.ProductionOrderReceivings.Update(productionOrderReceivings);
                }
                DBContext.SaveChanges();


                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.PalletCode == PalletCode
                                 select new PalletDetailsParam
                                 {
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletQty = r.Qty,
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     LaneCode = b.LaneCode
                                 }).FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object WarehouseLocation_PalletCheckOut(long UserID, string DeviceSerialNo, string applang, string PalletCode, long? ProductionQty)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            try
            {
                if (ProductionQty == null || string.IsNullOrEmpty(ProductionQty.ToString().Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية الانتاج" : "Missing Production Qty");
                    return new { responseStatus, palletDetails };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus, palletDetails };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
                    return new { responseStatus, palletDetails };
                }
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);

                if (PalletData.ProductionQtyCheckIn <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تسجيل دخول الباليت في موقع المخزن" : "Pallet is not checked in warehouse location");
                    return new { responseStatus, palletDetails };
                }
                if (PalletData != null)
                {
                    if (DBContext.Products.Any(x => x.ProductCode == PalletData.ProductCode && x.IsWarehouseLocation != true))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "لا يحتاج المنتج الموجود على الباليت المحددة إلى التخزين في موقع المخزن" : "Product that was in the specified pallet is not need to store in warehouse location");
                        return new { responseStatus, palletDetails };
                    }
                    PalletData.ProductionQtyCheckOut = ProductionQty;
                    PalletData.DeviceSerialNoCheckOut = DeviceSerialNo;
                    DBContext.PalletWips.Update(PalletData);
                }
                var productionOrderReceivings = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode && x.ProductionOrderId == PalletData.ProductionOrderId && x.BatchNo == PalletData.BatchNo && x.ProductionLineId == PalletData.ProductionLineId);
                if (productionOrderReceivings != null)
                {
                    productionOrderReceivings.ProductionQtyCheckOut = ProductionQty;
                    productionOrderReceivings.UserIdCheckOut = UserID;
                    productionOrderReceivings.DateTimeCheckOut = DateTime.Now;
                    productionOrderReceivings.DeviceSerialNoCheckOut = DeviceSerialNo;
                    DBContext.ProductionOrderReceivings.Update(productionOrderReceivings);
                }
                DBContext.SaveChanges();


                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.PalletCode == PalletCode
                                 select new PalletDetailsParam
                                 {
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),

                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletQty = r.Qty,
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     LaneCode = b.LaneCode
                                 }).FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<WarehouseReceivingDto> WarehouseReceivingAsync(long UserID, string DeviceSerialNo, string applang, string StorageLocationCode, long? CartoonReceivedQty, string PalletCode)
        {
            try
            {
                if (CartoonReceivedQty == null || string.IsNullOrEmpty(CartoonReceivedQty.ToString().Trim()))
                {
                    return new WarehouseReceivingDto{responseStatus =  ResponseStatusHelper.ErrorResponseStatus("Missing Cartoon Received Qty", "لم يتم إدخال الكمية المستلمه", applang) };
                }
                
                if (StorageLocationCode == null || string.IsNullOrEmpty(StorageLocationCode.Trim()))
                {
                   
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing storage location code", "لم يتم إدخال كود المخزن",applang) };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    
                    return new WarehouseReceivingDto{ responseStatus = ResponseStatusHelper.ErrorResponseStatus("Missing pallet code", "لم يتم إدخال كود الباليت",applang)};
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    return new WarehouseReceivingDto{ responseStatus = ResponseStatusHelper.ErrorResponseStatus("Pallet code is wrong", "كود الباليت خاطئ",applang) };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                   
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("Pallet is not loaded", "لم يتم تحميل الباليت",applang) };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
                {
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("The pallet has already been received in the warehouse", "تم استلام الباليت بالفعل في المستودع",applang) };
                }
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == PalletData.ProductCode);

                if (!DBContext.StorageLocations.Any(x => x.PlantCode == PalletData.PlantCode && x.StorageLocationCode == StorageLocationCode))
                {
                     return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("Storage Location Code is wrong or not related to the selected plant", "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره",applang) };
                }
                var NoBoxPerPallet = 0m;    
                if (ProductData != null)
                {
                    NoBoxPerPallet = ((ProductData.NumeratorforConversionPal ?? 0) / (ProductData.DenominatorforConversionPal ?? 0));
                }
                if (CartoonReceivedQty > NoBoxPerPallet)
                {
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus($"Cartoon Received Qty more than the allowed quantity {NoBoxPerPallet}", $"الكمية المستلمة اكثر من الكمية المسموح بها {NoBoxPerPallet}",applang) };
                }
                if (PalletData.StorageLocationCode != null)
                {

                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("The pallet has already been received in the warehouse", "تم استلام الباليت بالفعل في المستودع",applang) };
                }
                
                //send data to sap
                bool IsCreatedOnSap = false;
                //CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
                //    ProductionOrderDetailsData.ProductCode,
                //    ProductionOrderDetailsData.SapOrderId.ToString(),
                //    receivedQtyInUnits.Value.ToString(),
                //    ProductionOrderDetailsData.BatchNo,
                //    ProductionOrderDetailsData.ProductionDate.Value.ToString("yyyy-MM-dd"),
                //    ProductData.Uom,
                //    DateTime.Now.ToString("yyyy-MM-dd"),
                //    ProductionOrderDetailsData.PlantCode,
                //    StorageLocation
                //    );
                CloseBatchResponse Response = await SapService.CloseBatchAsync(
                    new CloseBatchParameters
                    {
                        MATNR = PalletData.ProductCode,
                        AUFNR = PalletData.SaporderId.ToString().PadLeft(12, '0'),
                        MENGE = CartoonReceivedQty.Value.ToString(),
                        CHARG = PalletData.BatchNo,
                        HSDAT = PalletData.ProductionDate.Value.ToString("yyyyMMdd"),
                        MEINS = ProductData.Uom,
                        BUDAT = DateTime.Now.ToString("yyyyMMdd"),
                        WERKS = PalletData.PlantCode,
                        LGORT = StorageLocationCode
                    }, "FwoB4c1CbI4-ODXyQaEGuQ==");
                if (Response != null && Response.messageType == "S")
                {
                    IsCreatedOnSap = true;
                    CloseBatch entity = new()
                    {
                        Uom = ProductData.Uom,
                        PlantCode = PalletData.PlantCode,
                        Storagelocation = StorageLocationCode,
                        BatchNumber = PalletData.BatchNo,
                        ProductCode = PalletData.ProductCode,
                        DateofManufacture = PalletData.ProductionDate,
                        SapOrderId = PalletData.SaporderId,
                        Qty = CartoonReceivedQty.Value,
                        PostingDateintheDocument = DateTime.Now,
                        UserIdAdd = UserID,
                        DateTimeAdd = DateTime.Now,
                        IsCreatedOnSap = IsCreatedOnSap,
                        MessageCode = Response.messageCode,
                        MessageText = Response.messageText,
                        NumberofMaterialDocument = Response.MBLNR,
                        MessageType = Response.messageType,
                        MaterialDocumentYear = Response.MJAHR,
                        Message = Response.message
                    };
                    DBContext.CloseBatchs.Add(entity);

                    var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode);
                    var NoItemPerBox = 0m;
                    var Package = 0;
                    if (ProductionOrderReceivingsData != null)
                    {
                        NoItemPerBox = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));
                        ProductionOrderReceivingsData.StorageLocationCode = StorageLocationCode;
                        ProductionOrderReceivingsData.WarehouseReceivingPackage = Package;
                        ProductionOrderReceivingsData.WarehouseReceivingCartoonReceivedQty = CartoonReceivedQty;
                        ProductionOrderReceivingsData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
                        ProductionOrderReceivingsData.IsWarehouseReceived = true;
                        ProductionOrderReceivingsData.UserIdWarehouse = UserID;
                        ProductionOrderReceivingsData.DateTimeWarehouse = DateTime.Now;
                        ProductionOrderReceivingsData.DeviceSerialNoWarehouse = DeviceSerialNo;
                        ProductionOrderReceivingsData.IsAddedInSap = IsCreatedOnSap;
                        DBContext.ProductionOrderReceivings.Update(ProductionOrderReceivingsData);
                    }
                    if (PalletData != null)
                    {
                        PalletData.StorageLocationCode = StorageLocationCode;
                        PalletData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
                        PalletData.IsWarehouseReceived = true;
                        PalletData.UserIdWarehouse = UserID;
                        PalletData.DateTimeWarehouse = DateTime.Now;
                        PalletData.DeviceSerialNoWarehouse = DeviceSerialNo;
                        if (PalletData.ReceivingQty != (CartoonReceivedQty * (long)NoItemPerBox))
                        {
                            ReceivingPalletsNeedApproval receivingPalletsNeedApproval = new ReceivingPalletsNeedApproval()
                            {
                                PalletCode = PalletData.PalletCode,
                                ProductionOrderId = PalletData.ProductionOrderId,
                                SaporderId = PalletData.SaporderId,
                                PlantCode = PalletData.PlantCode,
                                ProductionLineId = PalletData.ProductionLineId,
                                BatchNo = PalletData.BatchNo,
                                ProductCode = PalletData.ProductCode,
                                ProductionDate = PalletData.ProductionDate,
                                WarehouseReceivingQty = (CartoonReceivedQty * (long)NoItemPerBox),
                                WarehouseCartoonReceivingQty = CartoonReceivedQty,
                                UserIdWarehouse = UserID,
                                DateTimeWarehouse = DateTime.Now,
                                DeviceSerialNoWarehouse = DeviceSerialNo
                            };
                            DBContext.ReceivingPalletsNeedApprovals.Add(receivingPalletsNeedApproval);
                            PalletData.IsChangedQuantityByWarehouse = true;
                            DBContext.PalletWips.Update(PalletData);

                        }
                        DBContext.PalletWips.Update(PalletData);
                    }
                    var GetStock = DBContext.Stocks.FirstOrDefault(x => x.PlantCode == PalletData.PlantCode && x.ProductCode == PalletData.ProductCode && PalletData.ProductionDate.Value.Date == PalletData.ProductionDate.Value.Date && x.StorageLocationCode == StorageLocationCode);
                    if (GetStock != null)
                    {
                        GetStock.Qty += (Package + (CartoonReceivedQty * (long)NoItemPerBox));
                        DBContext.Stocks.Update(GetStock);
                    }
                    else
                    {
                        Stock stock = new()
                        {
                            PlantCode = PalletData.PlantCode,
                            ProductCode = PalletData.ProductCode,
                            StorageLocationCode = StorageLocationCode,
                            ProductionDate = PalletData.ProductionDate,
                            Qty = (Package + (CartoonReceivedQty * (long)NoItemPerBox))
                        };
                        DBContext.Stocks.Add(stock);
                    }
                    DBContext.SaveChanges();
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.SuccessResponseStatus("Data saved successfully", "تم حفظ البيانات بنجاح", applang) };
                }
                else
                {
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("Error from sap\nOrder number " + PalletData.SaporderId + "\n" + Response.messageText + " - " + Response.message, "خطأ من ساب\n طلب رقم" + PalletData.SaporderId + "\n" + Response.messageText + " - " + Response.message, applang) };
                }

            }
            catch (Exception ex)
            {
                return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ExceptionResponseStatus(ex.Message,applang) };
            }
        }
        //[Route("[action]")]
        //[HttpGet]
        //public object WarehouseReceiving(long UserID, string DeviceSerialNo, string applang, string StorageLocationCode, long? CartoonReceivedQty, long? Package, string PalletCode)
        //{
        //    ResponseStatus responseStatus = new();
        //    PalletDetailsParam palletDetails = new();
        //    try
        //    {
        //        decimal NoItemPerBox = 0;
        //        if (CartoonReceivedQty == null || string.IsNullOrEmpty(CartoonReceivedQty.ToString().Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية المستلمه" : "Missing Cartoon Received Qty");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (Package == null || string.IsNullOrEmpty(Package.ToString().Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال التغليف" : "Missing Package");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (StorageLocationCode == null || string.IsNullOrEmpty(StorageLocationCode.Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المخزن" : "Missing storage location code");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived == true))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
        //            return new { responseStatus, palletDetails };
        //        }
        //        var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
        //        var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == PalletData.ProductCode);

        //        if (!DBContext.StorageLocations.Any(x => x.PlantCode == PalletData.PlantCode && x.StorageLocationCode == StorageLocationCode))
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره" : "Storage Location Code is wrong or not related to the selected plant");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (PalletData.StorageLocationCode != null)
        //        {
        //            responseStatus.StatusCode = 401;
        //            responseStatus.IsSuccess = false;
        //            responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been received in the warehouse");
        //            return new { responseStatus, palletDetails };
        //        }
        //        if (PalletData.IsChangedQuantityByWarehouse == true)
        //        {
        //            if (DBContext.ReceivingPalletsNeedApprovals.Any(x => x.PalletCode == PalletData.PalletCode && x.IsProductionApproved == null))
        //            {
        //                responseStatus.StatusCode = 401;
        //                responseStatus.IsSuccess = false;
        //                responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم تغيير الكمية المستلمة للباليت من قبل مستخدم المستودع وتحتاج الى موافقة الانتاج" : "The quantity received for the pallet has been changed by the warehouse user and needs production approval");
        //                return new { responseStatus, palletDetails };
        //            }

        //            var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode);
        //            if (ProductionOrderReceivingsData != null)
        //            {
        //                NoItemPerBox = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));

        //                ProductionOrderReceivingsData.StorageLocationCode = StorageLocationCode;
        //                ProductionOrderReceivingsData.WarehouseReceivingPackage = Package;
        //                ProductionOrderReceivingsData.WarehouseReceivingCartoonReceivedQty = CartoonReceivedQty;
        //                ProductionOrderReceivingsData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                ProductionOrderReceivingsData.IsWarehouseReceived = true;
        //                ProductionOrderReceivingsData.UserIdWarehouse = UserID;
        //                ProductionOrderReceivingsData.DateTimeWarehouse = DateTime.Now;
        //                ProductionOrderReceivingsData.DeviceSerialNoWarehouse = DeviceSerialNo;
        //                DBContext.ProductionOrderReceivings.Update(ProductionOrderReceivingsData);
        //            }
        //            if (PalletData != null)
        //            {
        //                PalletData.StorageLocationCode = StorageLocationCode;
        //                PalletData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                PalletData.IsWarehouseReceived = true;
        //                PalletData.UserIdWarehouse = UserID;
        //                PalletData.DateTimeWarehouse = DateTime.Now;
        //                PalletData.DeviceSerialNoWarehouse = DeviceSerialNo;
        //                DBContext.PalletWips.Update(PalletData);
        //            }
        //            var GetStock = DBContext.Stocks.FirstOrDefault(x => x.PlantCode == PalletData.PlantCode && x.ProductCode == PalletData.ProductCode && PalletData.ProductionDate.Value.Date == PalletData.ProductionDate.Value.Date && x.StorageLocationCode == StorageLocationCode);
        //            if (GetStock != null)
        //            {
        //                GetStock.Qty += (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                DBContext.Stocks.Update(GetStock);
        //            }
        //            else
        //            {
        //                Stock stock = new()
        //                {
        //                    PlantCode = PalletData.PlantCode,
        //                    ProductCode = PalletData.ProductCode,
        //                    StorageLocationCode = StorageLocationCode,
        //                    ProductionDate = PalletData.ProductionDate,
        //                    Qty = (Package + (CartoonReceivedQty * (long)NoItemPerBox))
        //                };
        //                DBContext.Stocks.Add(stock);
        //            }
        //            //send data to sap
                    
        //            bool IsCreatedOnSap = false;
                    

        //            //add or update close batch table
        //            var closeBatchEntity = DBContext.CloseBatchs.FirstOrDefault(x => x.BatchNumber == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode);

        //            if (closeBatchEntity != null)
        //            {
        //                var warehouseQtyAfterClose = (closeBatchEntity.Qty + Package + CartoonReceivedQty * NoItemPerBox);
        //                CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
        //                    PalletData.ProductCode,
        //                    PalletData.SaporderId.ToString(),
        //                    warehouseQtyAfterClose.ToString(),
        //                    PalletData.BatchNo,
        //                    PalletData.ProductionDate.Value.ToString("yyyy-MM-dd"),
        //                    ProductData.Uom,
        //                    DateTime.Now.ToString("yyyy-MM-dd"),
        //                    PalletData.PlantCode,
        //                    StorageLocationCode
        //                );
        //                if (Response != null && Response.messageType == "S")
        //                {
        //                    IsCreatedOnSap = true;
        //                }
        //                closeBatchEntity.Qty = (int) warehouseQtyAfterClose;
        //                closeBatchEntity.IsCreatedOnSap = IsCreatedOnSap;
        //                closeBatchEntity.MessageCode = Response.messageCode;
        //                closeBatchEntity.MessageText = Response.messageText;
        //                closeBatchEntity.MaterialDocumentYear = Response.MJAHR;
        //                closeBatchEntity.Message = Response.message;
        //                DBContext.CloseBatchs.Update(closeBatchEntity);
        //            }
        //            else
        //            {
        //                var warehouseQtyAfterClose = (Package + CartoonReceivedQty * NoItemPerBox);
        //                CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
        //                    PalletData.ProductCode,
        //                    PalletData.SaporderId.ToString(),
        //                    warehouseQtyAfterClose.ToString(),
        //                    PalletData.BatchNo,
        //                    PalletData.ProductionDate.Value.ToString("yyyy-MM-dd"),
        //                    ProductData.Uom,
        //                    DateTime.Now.ToString("yyyy-MM-dd"),
        //                    PalletData.PlantCode,
        //                    StorageLocationCode
        //                );
        //                if (Response != null && Response.messageType == "S")
        //                {
        //                    IsCreatedOnSap = true;
        //                }
        //                CloseBatch entity = new()
        //                {
        //                    Uom = ProductData.Uom,
        //                    PlantCode = PalletData.PlantCode,
        //                    Storagelocation = StorageLocationCode,
        //                    BatchNumber = PalletData.BatchNo,
        //                    ProductCode = PalletData.ProductCode,
        //                    DateofManufacture = PalletData.ProductionDate,
        //                    SapOrderId = PalletData.SaporderId,
        //                    Qty = (int)warehouseQtyAfterClose,
        //                    PostingDateintheDocument = DateTime.Now,
        //                    UserIdAdd = UserID,
        //                    DateTimeAdd = DateTime.Now,
        //                    IsCreatedOnSap = IsCreatedOnSap,
        //                    MessageCode = Response.messageCode,
        //                    MessageText = Response.messageText,
        //                    NumberofMaterialDocument = Response.MBLNR,
        //                    MessageType = Response.messageType,
        //                    MaterialDocumentYear = Response.MJAHR,
        //                    Message = Response.message,
        //                };
        //                DBContext.CloseBatchs.Add(entity);

        //            }

        //            DBContext.SaveChanges();


        //        }
        //        else
        //        {

        //            if (PalletData != null)
        //            {
        //                var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode);
        //                if (ProductionOrderReceivingsData != null)
        //                {
        //                    NoItemPerBox = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));

        //                    if (PalletData.ReceivingQty != (CartoonReceivedQty * (long)NoItemPerBox))
        //                    {
        //                        ReceivingPalletsNeedApproval receivingPalletsNeedApproval = new ReceivingPalletsNeedApproval()
        //                        {
        //                            PalletCode = PalletData.PalletCode,
        //                            ProductionOrderId = PalletData.ProductionOrderId,
        //                            SaporderId = PalletData.SaporderId,
        //                            PlantCode = PalletData.PlantCode,
        //                            ProductionLineId = PalletData.ProductionLineId,
        //                            BatchNo = PalletData.BatchNo,
        //                            ProductCode = PalletData.ProductCode,
        //                            ProductionDate = PalletData.ProductionDate,
        //                            WarehouseReceivingQty = (CartoonReceivedQty * (long)NoItemPerBox),
        //                            WarehouseCartoonReceivingQty = CartoonReceivedQty,
        //                            UserIdWarehouse = UserID,
        //                            DateTimeWarehouse = DateTime.Now,
        //                            DeviceSerialNoWarehouse = DeviceSerialNo
        //                        };
        //                        DBContext.ReceivingPalletsNeedApprovals.Add(receivingPalletsNeedApproval);
        //                        PalletData.IsChangedQuantityByWarehouse = true;
        //                        DBContext.PalletWips.Update(PalletData);
        //                        DBContext.SaveChanges();

        //                    }
        //                    else
        //                    {

        //                        ProductionOrderReceivingsData.StorageLocationCode = StorageLocationCode;
        //                        ProductionOrderReceivingsData.WarehouseReceivingPackage = Package;
        //                        ProductionOrderReceivingsData.WarehouseReceivingCartoonReceivedQty = CartoonReceivedQty;
        //                        ProductionOrderReceivingsData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                        ProductionOrderReceivingsData.IsWarehouseReceived = true;
        //                        ProductionOrderReceivingsData.UserIdWarehouse = UserID;
        //                        ProductionOrderReceivingsData.DateTimeWarehouse = DateTime.Now;
        //                        ProductionOrderReceivingsData.DeviceSerialNoWarehouse = DeviceSerialNo;
        //                        DBContext.ProductionOrderReceivings.Update(ProductionOrderReceivingsData);

        //                        PalletData.StorageLocationCode = StorageLocationCode;
        //                        PalletData.WarehouseReceivingQty = (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                        PalletData.IsWarehouseReceived = true;
        //                        PalletData.UserIdWarehouse = UserID;
        //                        PalletData.DateTimeWarehouse = DateTime.Now;
        //                        PalletData.DeviceSerialNoWarehouse = DeviceSerialNo;
        //                        DBContext.PalletWips.Update(PalletData);
        //                        var GetStock = DBContext.Stocks.FirstOrDefault(x => x.PlantCode == PalletData.PlantCode && x.ProductCode == PalletData.ProductCode && PalletData.ProductionDate.Value.Date == PalletData.ProductionDate.Value.Date && x.StorageLocationCode == StorageLocationCode);
        //                        if (GetStock != null)
        //                        {
        //                            GetStock.Qty += (Package + (CartoonReceivedQty * (long)NoItemPerBox));
        //                            DBContext.Stocks.Update(GetStock);
        //                        }
        //                        else
        //                        {
        //                            Stock stock = new()
        //                            {
        //                                PlantCode = PalletData.PlantCode,
        //                                ProductCode = PalletData.ProductCode,
        //                                StorageLocationCode = StorageLocationCode,
        //                                ProductionDate = PalletData.ProductionDate,
        //                                Qty = (Package + (CartoonReceivedQty * (long)NoItemPerBox))
        //                            };
        //                            DBContext.Stocks.Add(stock);
        //                        }
        //                        bool IsCreatedOnSap = false;


        //                        //add or update close batch table
        //                        var closeBatchEntity = DBContext.CloseBatchs.FirstOrDefault(x => x.BatchNumber == PalletData.BatchNo && x.ProductCode == PalletData.ProductCode);

        //                        if (closeBatchEntity != null)
        //                        {
        //                            var warehouseQtyAfterClose = (closeBatchEntity.Qty + Package + CartoonReceivedQty * NoItemPerBox);
        //                            CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
        //                                PalletData.ProductCode,
        //                                PalletData.SaporderId.ToString(),
        //                                warehouseQtyAfterClose.ToString(),
        //                                PalletData.BatchNo,
        //                                PalletData.ProductionDate.Value.ToString("yyyy-MM-dd"),
        //                                ProductData.Uom,
        //                                DateTime.Now.ToString("yyyy-MM-dd"),
        //                                PalletData.PlantCode,
        //                                StorageLocationCode
        //                            );
        //                            if (Response != null && Response.messageType == "S")
        //                            {
        //                                IsCreatedOnSap = true;
        //                            }
        //                            closeBatchEntity.Qty = (int)warehouseQtyAfterClose;
        //                            closeBatchEntity.IsCreatedOnSap = IsCreatedOnSap;
        //                            closeBatchEntity.MessageCode = Response.messageCode;
        //                            closeBatchEntity.MessageText = Response.messageText;
        //                            closeBatchEntity.MaterialDocumentYear = Response.MJAHR;
        //                            closeBatchEntity.Message = Response.message;
        //                            DBContext.CloseBatchs.Update(closeBatchEntity);
        //                        }
        //                        else
        //                        {
        //                            var warehouseQtyAfterClose = (Package + CartoonReceivedQty * NoItemPerBox);
        //                            CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
        //                                PalletData.ProductCode,
        //                                PalletData.SaporderId.ToString(),
        //                                warehouseQtyAfterClose.ToString(),
        //                                PalletData.BatchNo,
        //                                PalletData.ProductionDate.Value.ToString("yyyy-MM-dd"),
        //                                ProductData.Uom,
        //                                DateTime.Now.ToString("yyyy-MM-dd"),
        //                                PalletData.PlantCode,
        //                                StorageLocationCode
        //                            );
        //                            if (Response != null && Response.messageType == "S")
        //                            {
        //                                IsCreatedOnSap = true;
        //                            }
        //                            CloseBatch entity = new()
        //                            {
        //                                Uom = ProductData.Uom,
        //                                PlantCode = PalletData.PlantCode,
        //                                Storagelocation = StorageLocationCode,
        //                                BatchNumber = PalletData.BatchNo,
        //                                ProductCode = PalletData.ProductCode,
        //                                DateofManufacture = PalletData.ProductionDate,
        //                                SapOrderId = PalletData.SaporderId,
        //                                Qty = (int)warehouseQtyAfterClose,
        //                                PostingDateintheDocument = DateTime.Now,
        //                                UserIdAdd = UserID,
        //                                DateTimeAdd = DateTime.Now,
        //                                IsCreatedOnSap = IsCreatedOnSap,
        //                                MessageCode = Response.messageCode,
        //                                MessageText = Response.messageText,
        //                                NumberofMaterialDocument = Response.MBLNR,
        //                                MessageType = Response.messageType,
        //                                MaterialDocumentYear = Response.MJAHR,
        //                                Message = Response.message,
        //                            };
        //                            DBContext.CloseBatchs.Add(entity);

        //                        }
        //                    }
        //                    DBContext.SaveChanges();
        //                }
        //            }


        //            palletDetails = (from b in DBContext.PalletWips
        //                             join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
        //                             join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
        //                             join p in DBContext.Products on b.ProductCode equals p.ProductCode
        //                             join x in DBContext.Plants on l.PlantCode equals x.PlantCode
        //                             join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
        //                             join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
        //                             where b.PalletCode == PalletCode
        //                             select new PalletDetailsParam
        //                             {
        //                                 PlantId = x.PlantId,
        //                                 PlantCode = x.PlantCode,
        //                                 PlantDesc = x.PlantDesc,
        //                                 OrderTypeId = o.OrderTypeId,
        //                                 OrderTypeCode = o.OrderTypeCode,
        //                                 OrderTypeDesc = o.OrderTypeDesc,
        //                                 ProductId = p.ProductId,
        //                                 ProductCode = p.ProductCode,
        //                                 ProductDesc = p.ProductDesc,
        //                                 NumeratorforConversionPac = p.NumeratorforConversionPac,
        //                                 NumeratorforConversionPal = p.NumeratorforConversionPal,
        //                                 DenominatorforConversionPal = p.DenominatorforConversionPal,
        //                                 DenominatorforConversionPac = p.DenominatorforConversionPac,
        //                                 ProductionOrderId = l.ProductionOrderId,
        //                                 SapOrderId = l.SapOrderId.Value,
        //                                 ProductionOrderQty = l.Qty,
        //                                 ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
        //                                 OrderDate = l.OrderDate,
        //                                 BatchNo = r.BatchNo,
        //                                 PalletQty = r.Qty,
        //                                 IsWarehouseLocation = b.IsWarehouseLocation,
        //                                 ProductionQtyCheckIn = b.ProductionQtyCheckIn,
        //                                 ProductionQtyCheckOut = b.ProductionQtyCheckOut,
        //                                 IsWarehouseReceived = b.IsWarehouseReceived,
        //                                 StorageLocationCode = b.StorageLocationCode,
        //                                 WarehouseReceivingQty = b.WarehouseReceivingQty,
        //                                 WarehouseReceivingCartoonQty = ((decimal)b.WarehouseReceivingQty / NoItemPerBox),
        //                                 LaneCode = b.LaneCode,
        //                                 WarehouseReceivingPackage = r.WarehouseReceivingPackage,
        //                                 WarehouseReceivingCartoonReceivedQty = r.WarehouseReceivingCartoonReceivedQty,
        //                                 IsChangedQuantityByWarehouse = b.IsChangedQuantityByWarehouse
        //                             }).FirstOrDefault();

        //            if (palletDetails != null)
        //            {
        //                responseStatus.StatusCode = 200;
        //                responseStatus.IsSuccess = true;
        //                responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
        //            }
        //            else
        //            {
        //                responseStatus.StatusCode = 400;
        //                responseStatus.IsSuccess = false;
        //                responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        responseStatus.StatusCode = 500;
        //        responseStatus.IsSuccess = false;
        //        responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
        //        responseStatus.ErrorMessage = ex.Message;
        //    }
        //    return new { responseStatus, palletDetails };
        //}
        [Route("[action]")]
        [HttpPost]
        public async Task<object> ReceiveAndPutAwayAsync(long UserID, string DeviceSerialNo, string applang, string StorageLocationCode, string LaneCode, string PalletCode, long? CartoonReceivedQty)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            try
            {
                if (string.IsNullOrWhiteSpace(LaneCode))
                    return ErrorResponse("لم يتم إدخال كود المسار", "Missing Lane Code");

                if (string.IsNullOrWhiteSpace(PalletCode))
                    return ErrorResponse("لم يتم إدخال كود الباليت", "Missing Pallet Code");

                if (CartoonReceivedQty == null)
                    return ErrorResponse("لم يتم إدخال الكمية المستلمة", "Missing Cartoon Received Qty");
                if (StorageLocationCode == null || string.IsNullOrEmpty(StorageLocationCode.Trim()))
                {
                    return ErrorResponse("لم يتم إدخال كود المخزن", "Missing storage location code");
                }
                var palletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                if (palletData == null || !DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                    return ErrorResponse("كود الباليت غير صالح أو لم يتم تحميل الباليت", "Invalid or unloaded Pallet Code");

                if (palletData.IsWarehouseReceived == true)
                    return ErrorResponse("تم استلام الباليت بالفعل في المستودع", "The pallet has already been received in the warehouse");
                var productData = DBContext.Products.FirstOrDefault(x => x.ProductCode == palletData.ProductCode);
                var NoBoxPerPallet = ((productData?.NumeratorforConversionPal ?? 0) / (productData?.DenominatorforConversionPal ?? 1));
                var NoUnitPerBox = ((productData?.NumeratorforConversionPac ?? 0) / (productData?.DenominatorforConversionPac ?? 1));
                if (CartoonReceivedQty> NoBoxPerPallet)
                    return ErrorResponse("الكمية المستلمة أكبر من كمية الكرتون في الباليت", "Received quantity exceeds the number of cartons in the pallet");
                var laneData = DBContext.Lanes.FirstOrDefault(x => x.PlantCode == palletData.PlantCode && x.StorageLocationCode == StorageLocationCode && x.LaneCode == LaneCode);
                if (laneData == null)
                    return ErrorResponse("كود المسار خاطئ أو لا ينتمي للمصنع أو المخزن", "Lane Code is invalid or not related to selected plant/storage location");
                if (!DBContext.StorageLocations.Any(x => x.PlantCode == palletData.PlantCode && x.StorageLocationCode == StorageLocationCode))
                {
                    return ErrorResponse("كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره", "Storage Location Code is wrong or not related to the selected plant");
                }
                var receivingData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.PalletCode == PalletCode);
                decimal NoItemPerBox = ((receivingData?.NumeratorforConversionPac ?? 0) / (receivingData?.DenominatorforConversionPac ?? 1));
                long totalQty = (long)(CartoonReceivedQty * NoItemPerBox);
                bool IsCreatedOnSap = false;
                //CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(
                //    ProductionOrderDetailsData.ProductCode,
                //    ProductionOrderDetailsData.SapOrderId.ToString(),
                //    receivedQtyInUnits.Value.ToString(),
                //    ProductionOrderDetailsData.BatchNo,
                //    ProductionOrderDetailsData.ProductionDate.Value.ToString("yyyy-MM-dd"),
                //    ProductData.Uom,
                //    DateTime.Now.ToString("yyyy-MM-dd"),
                //    ProductionOrderDetailsData.PlantCode,
                //    StorageLocation
                //    );
                CloseBatchResponse Response = await SapService.CloseBatchAsync(
                    new CloseBatchParameters
                    {
                        MATNR = palletData.ProductCode,
                        AUFNR = palletData.SaporderId.ToString().PadLeft(12, '0'),
                        MENGE = CartoonReceivedQty.Value.ToString(),
                        CHARG = palletData.BatchNo,
                        HSDAT = palletData.ProductionDate.Value.ToString("yyyyMMdd"),
                        MEINS = productData.Uom,
                        BUDAT = DateTime.Now.ToString("yyyyMMdd"),
                        WERKS = palletData.PlantCode,
                        LGORT = StorageLocationCode
                    }, "FwoB4c1CbI4-ODXyQaEGuQ==");
                if (Response != null && Response.messageType == "S")
                {
                    IsCreatedOnSap = true;

                    CloseBatch entity = new()
                    {
                        Uom = productData.Uom,
                        PlantCode = palletData.PlantCode,
                        Storagelocation = StorageLocationCode,
                        BatchNumber = palletData.BatchNo,
                        ProductCode = palletData.ProductCode,
                        DateofManufacture = palletData.ProductionDate,
                        SapOrderId = palletData.SaporderId,
                        Qty = CartoonReceivedQty.Value,
                        PostingDateintheDocument = DateTime.Now,
                        UserIdAdd = UserID,
                        DateTimeAdd = DateTime.Now,
                        IsCreatedOnSap = IsCreatedOnSap,
                        MessageCode = Response.messageCode,
                        MessageText = Response.messageText,
                        NumberofMaterialDocument = Response.MBLNR,
                        MessageType = Response.messageType,
                        MaterialDocumentYear = Response.MJAHR,
                        Message = Response.message
                    };
                    DBContext.CloseBatchs.Add(entity);
                    // Update PalletWip
                    palletData.WarehouseReceivingQty = totalQty;
                    palletData.IsWarehouseReceived = true;
                    palletData.UserIdWarehouse = UserID;
                    palletData.DateTimeWarehouse = DateTime.Now;
                    palletData.DeviceSerialNoWarehouse = DeviceSerialNo;
                    palletData.StorageLocationCode ??= StorageLocationCode;
                    palletData.WarehouseReceivingQty = CartoonReceivedQty * (int)NoItemPerBox;
                    palletData.LaneCode = LaneCode;
                    palletData.UserIdPutAway = UserID;
                    palletData.DateTimePutAway = DateTime.Now;
                    palletData.DeviceSerialNoPutAway = DeviceSerialNo;
                    if (palletData.ReceivingQty != (CartoonReceivedQty * (long)NoItemPerBox))
                    {
                        ReceivingPalletsNeedApproval receivingPalletsNeedApproval = new ReceivingPalletsNeedApproval()
                        {
                            PalletCode = palletData.PalletCode,
                            ProductionOrderId = palletData.ProductionOrderId,
                            SaporderId = palletData.SaporderId,
                            PlantCode = palletData.PlantCode,
                            ProductionLineId = palletData.ProductionLineId,
                            BatchNo = palletData.BatchNo,
                            ProductCode = palletData.ProductCode,
                            ProductionDate = palletData.ProductionDate,
                            WarehouseReceivingQty = (CartoonReceivedQty * (long)NoItemPerBox),
                            WarehouseCartoonReceivingQty = CartoonReceivedQty,
                            UserIdWarehouse = UserID,
                            DateTimeWarehouse = DateTime.Now,
                            DeviceSerialNoWarehouse = DeviceSerialNo
                        };
                        DBContext.ReceivingPalletsNeedApprovals.Add(receivingPalletsNeedApproval);
                        palletData.IsChangedQuantityByWarehouse = true;
                        DBContext.PalletWips.Update(palletData);

                    }
                    DBContext.PalletWips.Update(palletData);



                    // Update Receiving Data
                    if (receivingData != null)
                    {

                        receivingData.WarehouseReceivingQty = totalQty;
                        receivingData.IsWarehouseReceived = true;
                        receivingData.UserIdWarehouse = UserID;
                        receivingData.DateTimeWarehouse = DateTime.Now;
                        receivingData.DeviceSerialNoWarehouse = DeviceSerialNo;
                        receivingData.StorageLocationCode = palletData.StorageLocationCode;
                        receivingData.WarehouseReceivingCartoonReceivedQty = CartoonReceivedQty;
                        receivingData.LaneCode = LaneCode;
                        receivingData.UserIdPutAway = UserID;
                        receivingData.DateTimePutAway = DateTime.Now;
                        receivingData.DeviceSerialNoPutAway = DeviceSerialNo;
                        receivingData.IsAddedInSap = IsCreatedOnSap;

                        DBContext.ProductionOrderReceivings.Update(receivingData);
                    }

                    // Update or Add Stock
                    var stock = DBContext.Stocks.FirstOrDefault(x => x.PlantCode == palletData.PlantCode && x.ProductCode == palletData.ProductCode && x.ProductionDate == palletData.ProductionDate && x.StorageLocationCode == palletData.StorageLocationCode);
                    if (stock != null)
                    {
                        stock.Qty += totalQty;
                        DBContext.Stocks.Update(stock);
                    }
                    else
                    {
                        DBContext.Stocks.Add(new Stock
                        {
                            PlantCode = palletData.PlantCode,
                            ProductCode = palletData.ProductCode,
                            StorageLocationCode = palletData.StorageLocationCode,
                            ProductionDate = palletData.ProductionDate,
                            Qty = totalQty
                        });
                    }





                    DBContext.SaveChanges();

                    palletDetails = BuildPalletDetails(PalletCode, NoItemPerBox);

                    if (palletDetails != null)
                    {
                        responseStatus.StatusCode = 200;
                        responseStatus.IsSuccess = true;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                        return new { responseStatus, palletDetails };
                    }
                    else
                    {
                        return ErrorResponse("لم يتم تحميل الباليت", "Pallet is not loaded");
                    }
                }
                else
                {
                    return new WarehouseReceivingDto { responseStatus = ResponseStatusHelper.ErrorResponseStatus("Error from sap\nOrder number "+ palletData.SaporderId+ "\n" + Response.messageText + " - " + Response.message, "خطأ من ساب\n طلب رقم" +palletData.SaporderId+ "\n" + Response.messageText + " - " + Response.message, applang) };
                }


                //add or update close batch table

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
                return new { responseStatus };
            }

            return new { responseStatus, palletDetails };
            object ErrorResponse(string arMessage, string enMessage)
            {
                responseStatus.StatusCode = 401;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = (applang == "ar") ? arMessage : enMessage;
                return new { responseStatus, palletDetails };
            }

            PalletDetailsParam BuildPalletDetails(string palletCode, decimal NoItemPerBox)
            {
                return (from b in DBContext.PalletWips
                        join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                        join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                        join p in DBContext.Products on b.ProductCode equals p.ProductCode
                        join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                        join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                        join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                        where b.PalletCode == palletCode
                        select new PalletDetailsParam
                        {
                            PlantId = x.PlantId,
                            PlantCode = x.PlantCode,
                            PlantDesc = x.PlantDesc,
                            OrderTypeId = o.OrderTypeId,
                            OrderTypeCode = o.OrderTypeCode,
                            OrderTypeDesc = o.OrderTypeDesc,
                            ProductId = p.ProductId,
                            ProductCode = p.ProductCode,
                            ProductDesc = p.ProductDesc,
                            NumeratorforConversionPac = p.NumeratorforConversionPac,
                            NumeratorforConversionPal = p.NumeratorforConversionPal,
                            DenominatorforConversionPal = p.DenominatorforConversionPal,
                            DenominatorforConversionPac = p.DenominatorforConversionPac,
                            ProductionOrderId = l.ProductionOrderId,
                            SapOrderId = l.SapOrderId.Value,
                            ProductionOrderQty = l.Qty,
                            ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 1))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 1))),
                            OrderDate = l.OrderDate,
                            BatchNo = r.BatchNo,
                            ProductionDate = r.ProductionDate,
                            PalletQty = r.Qty,
                            IsWarehouseLocation = b.IsWarehouseLocation,
                            ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                            ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                            IsWarehouseReceived = b.IsWarehouseReceived,
                            StorageLocationCode = b.StorageLocationCode,
                            WarehouseReceivingQty = b.WarehouseReceivingQty,
                            LaneCode = b.LaneCode,
                            PalletCode = r.PalletCode,
                            IsChangedQuantityByWarehouse = b.IsChangedQuantityByWarehouse,
                            WarehouseReceivingCartoonQty = (decimal)b.WarehouseReceivingQty / NoItemPerBox
                        }).FirstOrDefault();
            }
        }

        [Route("[action]")]
        [HttpGet]
        public object PutAway(long UserID, string DeviceSerialNo, string applang, string LaneCode, string PalletCode)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam palletDetails = new();
            try
            {

                if (LaneCode == null || string.IsNullOrEmpty(LaneCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing Lane Code");
                    return new { responseStatus, palletDetails };
                }
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus, palletDetails };
                }
                if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode && x.IsWarehouseReceived != true))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم تم استلام الباليت في المستودع" : "The pallet is not received yet in the warehouse");
                    return new { responseStatus, palletDetails };
                }
                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                var LaneData = DBContext.Lanes.FirstOrDefault(x => x.PlantCode == PalletData.PlantCode && x.StorageLocationCode == PalletData.StorageLocationCode && x.LaneCode == LaneCode);

                if (LaneData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المسار خاطئ او لا ينتمي للمصنع او المخزن التي تم اختيارهم" : "Lane Code is wrong or not related to the selected plant, storage location");
                    return new { responseStatus, palletDetails };
                }
                if (PalletData.LaneCode != null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم استلام الباليت بالفعل في المستودع" : "The pallet has already been put away in the warehouse");
                    return new { responseStatus, palletDetails };
                }
                //if (DBContext.StockLocations.Any(x => x.LaneCode == LaneCode && x.ProductCode != PalletData.ProductCode))
                //{
                //    responseStatus.StatusCode = 401;
                //    responseStatus.IsSuccess = false;
                //    responseStatus.StatusMessage = ((applang == "ar") ? "هذا المسار به منتج آخر. لذلك لا يمكنك وضع منتج اخر فى هذا المسار" : "This lane has another product. So you can not put away this product");
                //    return new { responseStatus, palletDetails };
                //}
                var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == PalletData.ProductionLineId && x.BatchNo == PalletData.BatchNo && x.ProductionOrderId == PalletData.ProductionOrderId && x.ProductCode == PalletData.ProductCode && x.PalletCode == PalletCode && x.IsWarehouseReceived == true);
                if (ProductionOrderReceivingsData != null)
                {
                    ProductionOrderReceivingsData.LaneCode = LaneCode;
                    ProductionOrderReceivingsData.UserIdPutAway = UserID;
                    ProductionOrderReceivingsData.DateTimePutAway = DateTime.Now;
                    ProductionOrderReceivingsData.DeviceSerialNoPutAway = DeviceSerialNo;
                    DBContext.ProductionOrderReceivings.Update(ProductionOrderReceivingsData);
                }
                if (PalletData != null)
                {
                    PalletData.LaneCode = LaneCode;
                    PalletData.UserIdPutAway = UserID;
                    PalletData.DateTimePutAway = DateTime.Now;
                    PalletData.DeviceSerialNoPutAway = DeviceSerialNo;
                    DBContext.PalletWips.Update(PalletData);
                }
                decimal NoItemPerBox = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));
                decimal NoBoxPerPallet = ((ProductionOrderReceivingsData.NumeratorforConversionPal ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPal ?? 0));
                decimal MaxItemQtyPerPallet = NoBoxPerPallet * NoItemPerBox;

                var GetStock = DBContext.Stocks.FirstOrDefault(x => x.PlantCode == PalletData.PlantCode && x.ProductCode == PalletData.ProductCode && PalletData.ProductionDate.Value.Date == PalletData.ProductionDate.Value.Date && x.StorageLocationCode == PalletData.StorageLocationCode);
                if (GetStock != null)
                {
                    var GetStockLoc = DBContext.StockLocations.FirstOrDefault(x => x.StockId == GetStock.StockId && x.PlantCode == PalletData.PlantCode && x.ProductCode == PalletData.ProductCode && PalletData.ProductionDate.Value.Date == PalletData.ProductionDate.Value.Date && x.StorageLocationCode == PalletData.StorageLocationCode && x.LaneCode == LaneCode);
                    if (GetStockLoc != null)
                    {
                        GetStockLoc.Qty += PalletData.WarehouseReceivingQty;
                        if (GetStockLoc.Qty > (LaneData.NoOfPallets * MaxItemQtyPerPallet))
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك تجاوز الحد الأقصى للباليت لكل مسار" : "You can not exceed the limit no of pallet per lane");
                            return new { responseStatus, palletDetails };
                        }
                        DBContext.StockLocations.Update(GetStockLoc);
                    }
                    else
                    {
                        if (PalletData.WarehouseReceivingQty > (LaneData.NoOfPallets * MaxItemQtyPerPallet))
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك تجاوز الحد الأقصى للباليت لكل مسار" : "You can not exceed the limit no of pallet per lane");
                            return new { responseStatus, palletDetails };
                        }
                        StockLocation stockLocation = new()
                        {
                            StockId = GetStock.StockId,
                            PlantCode = PalletData.PlantCode,
                            ProductCode = PalletData.ProductCode,
                            StorageLocationCode = PalletData.StorageLocationCode,
                            LaneCode = LaneCode,
                            ProductionDate = PalletData.ProductionDate,
                            Qty = PalletData.WarehouseReceivingQty
                        };
                        DBContext.StockLocations.Add(stockLocation);
                    }
                }

                DBContext.SaveChanges();

                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.PalletCode == PalletCode
                                 select new PalletDetailsParam
                                 {
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     NumeratorforConversionPac = p.NumeratorforConversionPac,
                                     NumeratorforConversionPal = p.NumeratorforConversionPal,
                                     DenominatorforConversionPal = p.DenominatorforConversionPal,
                                     DenominatorforConversionPac = p.DenominatorforConversionPac,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     ProductionDate = r.ProductionDate,
                                     PalletQty = r.Qty,
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                                     ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     LaneCode = b.LaneCode,
                                     PalletCode = r.PalletCode
                                 }).FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        //Mobile use it to get purchase requisition list. (We can edit this api to get all purchase requisition from SAP then insert it like (SapPurchaseRequisition_Insert))
        public object GetPurchaseRequisitionList(long UserID, string DeviceSerialNo, string applang, string PlantCode, string StorageLocationCode, string PurchaseRequisitionReleaseDate)
        {
            ResponseStatus responseStatus = new();
            List<PurchaseRequisition_Details> GetList = new();
            List<SapPurchaseRequest_Response> SapPurchaseRequest = new();
            List<SapPurchaseRequest_Response> ErrorLog = new(); try
            {
                if (PurchaseRequisitionReleaseDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال تاريخ العملية" : "Missing Purchase Requisition Release Date");
                    return new { responseStatus, GetList };
                }
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, GetList };
                }

                SapPurchaseRequest = SAPIntegrationAPI.GetSapPurchaseRequest(PurchaseRequisitionReleaseDate, PlantCode, string.Empty, string.Empty);
                if (SapPurchaseRequest != null && SapPurchaseRequest.Count > 0)
                {
                    foreach (var item in SapPurchaseRequest)
                    {
                        try
                        {
                            if (item.BANFN == null || string.IsNullOrEmpty(item.BANFN.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال رقم طلب الشراء" : "Missing purchase requisition no");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.FRGDT == null)
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال تاريخ إصدار طلب الشراء" : "Missing purchase requisition release date");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.MENGE == null || double.Parse(item.MENGE) <= 0)
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كمية طلب الشراء" : "Missing purchase requisition quantity");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.WERKS == null || string.IsNullOrEmpty(item.WERKS.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود المصنع - المصدر" : "Missing plant code - source");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (!DBContext.Plants.Any(x => x.PlantCode == item.WERKS))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "كود المصنع خاطئ - المصدر" : "Plant code is wrong - source");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.LGORT != null && !string.IsNullOrEmpty(item.LGORT.Trim()))
                            {
                                if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.LGORT && x.PlantCode == item.WERKS))
                                {
                                    item.ErrorMsg = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره - المصدر" : "Storage Location Code is wrong or not related to the selected plant - source");
                                    ErrorLog.Add(item);
                                    continue;
                                }
                            }
                            //if (item.PlantCode_Destination == null || string.IsNullOrEmpty(item.PlantCode_Destination.Trim()))
                            //{
                            //    item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود المصنع - الوجهة" : "Missing plant code - destination");
                            //    ErrorLog.Add(item);
                            //    continue;
                            //}
                            //if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode_Destination))
                            //{
                            //    item.ErrorMsg = ((applang == "ar") ? "كود المصنع خاطئ - الوجهة" : "Plant code is wrong - destination");
                            //    ErrorLog.Add(item);
                            //    continue;
                            //}

                            //if (item.StorageLocationCode_Destination != null && !string.IsNullOrEmpty(item.StorageLocationCode_Destination.Trim()))
                            //{
                            //    if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == item.StorageLocationCode_Destination && x.PlantCode == item.PlantCode_Destination))
                            //    {
                            //        item.ErrorMsg = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره - الوجهة" : "Storage Location Code is wrong or not related to the selected plant - destination");
                            //        ErrorLog.Add(item);
                            //        continue;
                            //    }
                            //}

                            if (item.BNFPO == null || long.Parse(item.BNFPO) <= 0)
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال رقم السطر" : "Missing line number");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.MATNR == null || string.IsNullOrEmpty(item.MATNR.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                                ErrorLog.Add(item);
                                continue;
                            }

                            if (!DBContext.Products.Any(x => x.ProductCode == item.MATNR && x.PlantCode == item.WERKS))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "كود المنتج خاطئ او لا ينتمي للمصنع التي تم اختياره - المصدر" : "Product code is wrong or not related to the selected plant - source");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.MENGE == null || double.Parse(item.MENGE) <= 0)
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كمية المنتج" : "Missing product quantity");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.MEINS == null || string.IsNullOrEmpty(item.MEINS.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال وحدة قياس" : "Missing unit of measurement");
                                ErrorLog.Add(item);
                                continue;
                            }

                            long PurchaseRequisitionId_Inserted = 0;

                            if (DBContext.PurchaseRequisitions.Any(x => x.PurchaseRequisitionNo == item.BANFN))
                            {
                                var entity = DBContext.PurchaseRequisitions.FirstOrDefault(x => x.PurchaseRequisitionNo == item.BANFN);

                                entity.PurchaseRequisitionQty = (long)Math.Truncate(double.Parse(item.MENGE));
                                entity.PlantCodeSource = item.WERKS;
                                entity.StorageLocationCodeSource = item.LGORT;
                                //entity.PlantCodeDestination = item.PlantCode_Destination;
                                //entity.StorageLocationCodeDestination = item.StorageLocationCode_Destination;
                                entity.PurchaseRequisitionReleaseDate = DateTime.Parse(item.FRGDT);

                                DBContext.PurchaseRequisitions.Update(entity);
                                DBContext.SaveChanges();
                                PurchaseRequisitionId_Inserted = entity.PurchaseRequisitionId;
                            }
                            else
                            {
                                PurchaseRequisition entity = new()
                                {
                                    PurchaseRequisitionNo = item.BANFN,
                                    PurchaseRequisitionQty = (long)Math.Truncate(double.Parse(item.MENGE)),
                                    PlantCodeSource = item.WERKS,
                                    StorageLocationCodeSource = item.LGORT,
                                    //PlantCodeDestination = item.PlantCode_Destination,
                                    //StorageLocationCodeDestination = item.StorageLocationCode_Destination,
                                    PurchaseRequisitionReleaseDate = DateTime.Parse(item.FRGDT),
                                    PurchaseRequisitionStatus = "New",
                                    UserIdAdd = 2
                                };
                                DBContext.PurchaseRequisitions.Add(entity);
                                DBContext.SaveChanges();
                                PurchaseRequisitionId_Inserted = entity.PurchaseRequisitionId;
                            }



                            if (DBContext.PurchaseRequisitionDetails.Any(x => x.PurchaseRequisitionNo == item.BANFN && x.ProductCode == item.MATNR && x.LineNumber == long.Parse(item.BNFPO)))
                            {
                                var entity = DBContext.PurchaseRequisitionDetails.FirstOrDefault(x => x.PurchaseRequisitionNo == item.BANFN && x.ProductCode == item.MATNR);

                                entity.Qty = (long)Math.Truncate(double.Parse(item.MENGE));
                                entity.Uom = item.MEINS;
                                entity.LineNumber = long.Parse(item.BNFPO);

                                DBContext.PurchaseRequisitionDetails.Update(entity);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                if (DBContext.PurchaseRequisitionDetails.Any(x => x.PurchaseRequisitionNo == item.BANFN && x.LineNumber == long.Parse(item.BNFPO) && x.ProductCode != item.MATNR))
                                {
                                    item.ErrorMsg = ((applang == "ar") ? "رقم السطر هذا موجود بالفعل لكود منتج آخر لأمر الشراء المحدد" : "This line number already exists for another product code for the given purchase requisition");
                                    ErrorLog.Add(item);
                                    continue;
                                }

                                PurchaseRequisitionDetail entity = new()
                                {
                                    PurchaseRequisitionNo = item.BANFN,
                                    PurchaseRequisitionId = PurchaseRequisitionId_Inserted,
                                    LineNumber = long.Parse(item.BNFPO),
                                    LineStatus = "New",
                                    ProductCode = item.MATNR,
                                    Uom = item.MEINS,
                                    UserIdAdd = 2,
                                    Qty = (long)Math.Truncate(double.Parse(item.MENGE))
                                };
                                DBContext.PurchaseRequisitionDetails.Add(entity);
                                DBContext.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            item.ErrorMsg = "Exception Error: " + ex.Message;
                            ErrorLog.Add(item);
                            continue;
                        }
                    }
                }

                DateTime date = DateTime.ParseExact(PurchaseRequisitionReleaseDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                GetList = (from b in DBContext.PurchaseRequisitions
                           join d in DBContext.PurchaseRequisitionDetails on b.PurchaseRequisitionId equals d.PurchaseRequisitionId
                           join pro in DBContext.Products on d.ProductCode equals pro.ProductCode
                           where b.PlantCodeSource == PlantCode && b.PurchaseRequisitionReleaseDate.Value.Date == date.Date
                           && b.IsClosed != true && d.IsClosed != true && b.IsShipment != true && d.IsShipment != true && d.IsInProgress == true
                           select new PurchaseRequisition_Details
                           {
                               PurchaseRequisitionId = b.PurchaseRequisitionId,
                               PurchaseRequisitionNo = b.PurchaseRequisitionNo,
                               PurchaseRequisitionReleaseDate = b.PurchaseRequisitionReleaseDate,
                               PurchaseRequisitionQty = b.PurchaseRequisitionQty,
                               PlantCodeSource = b.PlantCodeSource,
                               StorageLocationCodeSource = b.StorageLocationCodeSource,
                               PlantCodeDestination = b.PlantCodeDestination,
                               StorageLocationCodeDestination = b.StorageLocationCodeDestination,
                               PurchaseRequisitionStatus = b.PurchaseRequisitionStatus,
                               ProductCode = d.ProductCode,
                               ProductDesc = ((applang == "ar") ? pro.ProductDescAr : pro.ProductDesc),
                               Qty = d.Qty,
                               PickupQty = d.PickupQty,
                               NumberOfCartonsPerPallet = (int)((pro.NumeratorforConversionPal ?? 0) / (pro.DenominatorforConversionPal ?? 0)),
                               Uom = d.Uom,
                               LineNumber = d.LineNumber,
                               LineStatus = d.LineStatus
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPallets_PutAway(long UserID, string DeviceSerialNo, string applang, string PlantCode, string StorageLocationCode, string ProductCode)
        {
            ResponseStatus responseStatus = new();
            List<PalletDetailsParam> GetList = new();
            try
            {

                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, GetList };
                }
                if (StorageLocationCode == null || string.IsNullOrEmpty(StorageLocationCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المخزن" : "Missing storage location code");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.StorageLocations.Any(x => x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره" : "Storage Location Code is wrong or not related to the selected plant");
                    return new { responseStatus, GetList };
                }
                if (ProductCode == null || string.IsNullOrEmpty(ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود النتج خاطئ " : "Product Code is wrong");
                    return new { responseStatus };
                }

                GetList = (from b in DBContext.PalletWips
                           join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                           join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                           join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                           where r.LaneCode != null && r.PlantCode == PlantCode && r.StorageLocationCode == StorageLocationCode && r.ProductCode == ProductCode && b.IsPickup != true
                           select new PalletDetailsParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               NumeratorforConversionPac = p.NumeratorforConversionPac,
                               NumeratorforConversionPal = p.NumeratorforConversionPal,
                               DenominatorforConversionPal = p.DenominatorforConversionPal,
                               DenominatorforConversionPac = p.DenominatorforConversionPac,
                               NoCartoonPerPallet = (p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0),
                               ProductionOrderId = l.ProductionOrderId,
                               SapOrderId = l.SapOrderId.Value,
                               ProductionOrderQty = l.Qty,
                               ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               OrderDate = l.OrderDate,
                               BatchNo = r.BatchNo,
                               ProductionDate = r.ProductionDate,
                               PalletQty = r.Qty,
                               PalletCartoonQty = (r.Qty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               IsWarehouseLocation = b.IsWarehouseLocation,
                               ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                               ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                               IsWarehouseReceived = b.IsWarehouseReceived,
                               StorageLocationCode = b.StorageLocationCode,
                               WarehouseReceivingQty = b.WarehouseReceivingQty,
                               WarehouseReceivingCartoonQty = (decimal)(b.WarehouseReceivingQty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               LaneCode = b.LaneCode,
                               PalletCode = r.PalletCode,
                               PickedupQtyFromPallet = b.PickedupQtyFromPallet,
                               PickedupCartoonQtyFromPallet = (b.PickedupQtyFromPallet / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               NoCanPerCartoon = ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))
                           }).ToList();

                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPallet_PutAway(long UserID, string DeviceSerialNo, string applang, string PalletCode)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam GetData = new();
            try
            {

                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, GetData };
                }


                GetData = (from b in DBContext.PalletWips
                           join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                           join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                           join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                           where r.LaneCode != null && r.PalletCode == PalletCode && b.IsPickup != true
                           select new PalletDetailsParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               NumeratorforConversionPac = p.NumeratorforConversionPac,
                               NumeratorforConversionPal = p.NumeratorforConversionPal,
                               DenominatorforConversionPal = p.DenominatorforConversionPal,
                               DenominatorforConversionPac = p.DenominatorforConversionPac,
                               ProductionOrderId = l.ProductionOrderId,
                               SapOrderId = l.SapOrderId.Value,
                               ProductionOrderQty = l.Qty,
                               ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               OrderDate = l.OrderDate,
                               BatchNo = r.BatchNo,
                               ProductionDate = r.ProductionDate,
                               PalletQty = r.Qty,
                               PalletCartoonQty = (r.Qty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               IsWarehouseLocation = b.IsWarehouseLocation,
                               ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                               ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                               IsWarehouseReceived = b.IsWarehouseReceived,
                               StorageLocationCode = b.StorageLocationCode,
                               WarehouseReceivingQty = b.WarehouseReceivingQty,
                               WarehouseReceivingCartoonQty = (decimal)(b.WarehouseReceivingQty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),

                               LaneCode = b.LaneCode,
                               PalletCode = r.PalletCode,
                               PickedupQtyFromPallet = b.PickedupQtyFromPallet,
                               PickedupCartoonQtyFromPallet = (b.PickedupQtyFromPallet / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               NoCanPerCartoon = ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))
                           }).FirstOrDefault();

                if (GetData != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPickedList(long UserID, string DeviceSerialNo, string applang, string PurchaseRequisitionNo)
        {
            ResponseStatus responseStatus = new();
            List<RoutePalletParam> palletDetails = new();
            try
            {
                if (PurchaseRequisitionNo == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال رقم طلب الشراء" : "Missing Purchase Requisition Number");
                    return new { responseStatus };
                }
                var GetPurchaseRequisitionsData = DBContext.PurchaseRequisitions.FirstOrDefault(x => x.PurchaseRequisitionNo == PurchaseRequisitionNo);
                if (GetPurchaseRequisitionsData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "رقم طلب الشراء خاطئ" : "Purchase Requisition Number is wrong");
                    return new { responseStatus };
                }


                palletDetails = (from b in DBContext.PurchaseRequisitions
                                 join d in DBContext.PickUpTransactions on b.PurchaseRequisitionId equals d.PurchaseRequisitionId
                                 join plant in DBContext.Plants on b.PlantCodeDestination equals plant.PlantCode
                                 join product in DBContext.Products on d.ProductCode equals product.ProductCode
                                 where b.PurchaseRequisitionNo == PurchaseRequisitionNo
                                 select new RoutePalletParam
                                 {
                                     PurchaseRequisitionId = b.PurchaseRequisitionId,
                                     PurchaseRequisitionNo = b.PurchaseRequisitionNo,
                                     BatchNo = d.BatchNo,
                                     PlantCodeDestination = b.PlantCodeDestination,
                                     PlantDescDestination = plant.PlantDesc,
                                     ProductCode = product.ProductCode,
                                     ProductDesc = product.ProductDesc,
                                     ProductionDate = d.ProductionDate,
                                     PickupCartoonQty = d.PickupCartoonQty,
                                     PalletCode = d.PalletCode,
                                     IsShipped = d.IsShipped
                                 }).ToList();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object PickUp(long UserID, string DeviceSerialNo, string applang, long? PurchaseRequisitionId, string PlantCode, string StorageLocationCode, string ProductCode, long? PickupCartoonQty, string ProductionDate, string BatchNo, string PalletCode_Warehouse, string PalletCode_Pickup)
        {
            ResponseStatus responseStatus = new();
            PurchaseRequisition_Details GetData = new();
            try
            {
                if (PurchaseRequisitionId == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال رمز طلب الشراء" : "Missing Purchase Requisition Id");
                    return new { responseStatus, GetData };
                }
                var GetPurchaseRequisitionsData = DBContext.PurchaseRequisitions.FirstOrDefault(x => x.PurchaseRequisitionId == PurchaseRequisitionId);
                if (GetPurchaseRequisitionsData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "رمز طلب الشراء خاطئ" : "Purchase Requisition id is wrong");
                    return new { responseStatus, GetData };
                }
                if (GetPurchaseRequisitionsData.IsClosed == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "طلب الشراء مغلق" : "Purchase Requisition is closed");
                    return new { responseStatus, GetData };
                }
                if (GetPurchaseRequisitionsData.IsShipment == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم شحن طلب الشراء بالكامل" : "Purchase Requisition is shipped");
                    return new { responseStatus, GetData };
                }
                if (PickupCartoonQty == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية" : "Missing pickup cartoon quantity");
                    return new { responseStatus, GetData };
                }
                if (ProductionDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال تاريخ الطلب" : "Missing order date");
                    return new { responseStatus, GetData };
                }
                if (BatchNo == null || string.IsNullOrEmpty(BatchNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch no");
                    return new { responseStatus, GetData };
                }
                if (ProductCode == null || string.IsNullOrEmpty(ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus, GetData };
                }
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, GetData };
                }
                if (StorageLocationCode == null || string.IsNullOrEmpty(StorageLocationCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المخزن" : "Missing storage location code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.StorageLocations.Any(x => x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره" : "Storage Location Code is wrong or not related to the selected plant");
                    return new { responseStatus, GetData };
                }

                if (!DBContext.PurchaseRequisitions.Any(x => x.PurchaseRequisitionId == PurchaseRequisitionId && x.PlantCodeSource == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع لا ينتمي لامر الشراء التي تم اختياره" : "Plant code is not related to the selected Purchase Requisition");
                    return new { responseStatus, GetData };
                }
                var GetPurchaseRequisitionDetailsData = DBContext.PurchaseRequisitionDetails.FirstOrDefault(x => x.PurchaseRequisitionId == PurchaseRequisitionId && x.ProductCode == ProductCode);
                if (GetPurchaseRequisitionDetailsData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المنتج لا ينتمي لامر الشراء التي تم اختياره" : "Product code is not related to the selected Purchase Requisition");
                    return new { responseStatus, GetData };
                }
                if (GetPurchaseRequisitionDetailsData.IsClosed == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم غلق هذا المنتج التابع لطلب الشراء" : "Purchase Requisition Line is closed");
                    return new { responseStatus, GetData };
                }
                if (GetPurchaseRequisitionDetailsData.IsShipment == true)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لقد تم شحن هذا المنتج التابع لطلب الشراء" : "Purchase Requisition Line is shipped");
                    return new { responseStatus, GetData };
                }
                if (PalletCode_Warehouse == null || string.IsNullOrEmpty(PalletCode_Warehouse.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت التي فى المخزن" : "Warehouse Pallet Code is missing");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode_Warehouse))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت التي فى المخزن خاطئ" : "Warehouse Pallet Code is wrong");
                    return new { responseStatus, GetData };
                }
                var GetPallet_WarehouseData = DBContext.PalletWips.FirstOrDefault(x => x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode && x.PalletCode == PalletCode_Warehouse && x.LaneCode != null && x.ProductCode == ProductCode && x.ProductionDate.Value.Date == DateTime.Parse(ProductionDate).Date && x.IsPickup != true);
                if (GetPallet_WarehouseData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت التي فى المخزن خاطئ او لا ينتمي للمصنع التي تم اختياره أو لم يتم وضعه بالمخزن" : "Pallet Code is wrong or not related to the selected plant, storage location, product or is not putaway yet");
                    return new { responseStatus, GetData };
                }
                var ProductionOrderReceivingsData = DBContext.ProductionOrderReceivings.FirstOrDefault(x => x.ProductionLineId == GetPallet_WarehouseData.ProductionLineId && x.BatchNo == GetPallet_WarehouseData.BatchNo && x.ProductionOrderId == GetPallet_WarehouseData.ProductionOrderId && x.ProductCode == GetPallet_WarehouseData.ProductCode && x.PalletCode == GetPallet_WarehouseData.PalletCode);
                decimal NoCanPerCartoon = 0;
                decimal NoBoxPerPallet = 0;
                decimal MaxReceivingQty = 0;
                if (ProductionOrderReceivingsData != null)
                {
                    NoCanPerCartoon = ((ProductionOrderReceivingsData.NumeratorforConversionPac ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPac ?? 0));
                    NoBoxPerPallet = ((ProductionOrderReceivingsData.NumeratorforConversionPal ?? 0) / (ProductionOrderReceivingsData.DenominatorforConversionPal ?? 0));
                    MaxReceivingQty = NoBoxPerPallet * NoCanPerCartoon;
                }

                GetPallet_WarehouseData.PickedupQtyFromPallet += (PickupCartoonQty * (long)NoCanPerCartoon);

                if (GetPallet_WarehouseData.WarehouseReceivingQty < GetPallet_WarehouseData.PickedupQtyFromPallet)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكبر من كمية الباليت فى المخزن" : "You cannot pickup quantity more than the warehouse pallet quantity");
                    return new { responseStatus, GetData };
                }

                GetPurchaseRequisitionDetailsData.PickupQty += PickupCartoonQty;
                if (GetPurchaseRequisitionDetailsData.PickupQty > GetPurchaseRequisitionDetailsData.Qty)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكبر من كمية منتج فى طلب الشراء" : "You cannot pickup quantity more than the purchase requisition line quantity");
                    return new { responseStatus, GetData };
                }

                if (PalletCode_Pickup == null || string.IsNullOrEmpty(PalletCode_Pickup.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت التي سوف يتم تحميلها" : "Pickup Pallet Code is missing");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode_Pickup))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت التي سوف يتم تحميلها خاطئ" : "Pickup Pallet Code is wrong");
                    return new { responseStatus, GetData };
                }
                if (PalletCode_Pickup != PalletCode_Warehouse)
                {

                    if (DBContext.PalletWips.Any(x => x.PalletCode == PalletCode_Pickup && (x.BatchNo != BatchNo || x.ProductCode != ProductCode || x.PlantCode != PlantCode || x.StorageLocationCode != StorageLocationCode || x.ProductionDate.Value.Date != DateTime.Parse(ProductionDate).Date || x.IsPickup != true)))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت مستخدم من قبل لطلب شراء او منتج او باتش اخر" : "Pickup pallet code is already used in another purchase requisition, product or batch");
                        return new { responseStatus, GetData };
                    }


                    var GetPallet_PickupData = DBContext.PalletWips.FirstOrDefault(x => x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode && x.PalletCode == PalletCode_Pickup
                    && x.ProductCode == ProductCode && x.ProductionDate.Value.Date == DateTime.Parse(ProductionDate).Date && x.IsPickup == true && x.BatchNo == BatchNo);
                    if (GetPallet_PickupData != null)
                    {

                        GetPallet_PickupData.PickupQty += (PickupCartoonQty * (long)NoCanPerCartoon);

                        if (MaxReceivingQty < GetPallet_PickupData.PickupQty)
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكثر من سعة الباليت القصوى" : "You can't receive more than the maximum pallet capacity");
                            return new { responseStatus, GetData };
                        }

                        GetPallet_PickupData.PickupCartoonQty += PickupCartoonQty;
                        DBContext.PalletWips.Update(GetPallet_PickupData);
                    }
                    else
                    {

                        if (MaxReceivingQty < (PickupCartoonQty * (long)NoCanPerCartoon))
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            responseStatus.StatusMessage = ((applang == "ar") ? "لا يمكنك استلام كمية أكثر من سعة الباليت القصوى" : "You can't receive more than the maximum pallet capacity");
                            return new { responseStatus, GetData };
                        }

                        PalletWip palletWip_Pickup = new()
                        {
                            BatchNo = GetPallet_WarehouseData.BatchNo,
                            IsPickup = true,
                            PalletCode = PalletCode_Pickup,
                            PickupCartoonQty = PickupCartoonQty,
                            PickupQty = (PickupCartoonQty * (long)NoCanPerCartoon),
                            PlantCode = GetPallet_WarehouseData.PlantCode,
                            StorageLocationCode = GetPallet_WarehouseData.StorageLocationCode,
                            ProductCode = GetPallet_WarehouseData.ProductCode,
                            ProductionDate = DateTime.Parse(ProductionDate).Date,
                            ProductionLineId = GetPallet_WarehouseData.ProductionLineId,
                            ProductionOrderId = GetPallet_WarehouseData.ProductionOrderId,
                            SaporderId = GetPallet_WarehouseData.SaporderId
                        };

                        DBContext.PalletWips.Add(palletWip_Pickup);
                    }
                }
                else
                {
                    GetPallet_WarehouseData.PickupQty += (PickupCartoonQty * (long)NoCanPerCartoon);
                    GetPallet_WarehouseData.PickupCartoonQty += PickupCartoonQty;
                    DBContext.PalletWips.Update(GetPallet_WarehouseData);
                }

                var GetPickUpData = DBContext.PickUpTransactions.FirstOrDefault(x => x.PurchaseRequisitionId == PurchaseRequisitionId && x.BatchNo == BatchNo && x.ProductCode == ProductCode && x.ProductionDate.Value.Date == DateTime.Parse(ProductionDate).Date && x.PlantCode == PlantCode && x.StorageLocationCode == StorageLocationCode && x.PalletCode == PalletCode_Pickup);
                if (GetPickUpData != null)
                {
                    GetPickUpData.PickupCartoonQty += PickupCartoonQty;
                    GetPickUpData.PickupQty = (PickupCartoonQty * (long)NoCanPerCartoon);
                    GetPickUpData.PickUpTransactionStatus = "InProgress";
                    GetPickUpData.UserIdAdd = UserID;
                    GetPickUpData.DateTimeAdd = DateTime.Now;
                    GetPickUpData.DeviceSerialNoAdd = DeviceSerialNo;
                    DBContext.PickUpTransactions.Update(GetPickUpData);
                }
                else
                {
                    PickUpTransaction pickUpTransaction = new PickUpTransaction()
                    {
                        PurchaseRequisitionId = PurchaseRequisitionId,
                        PlantCode = PlantCode,
                        StorageLocationCode = StorageLocationCode,
                        ProductCode = ProductCode,
                        NoCanPerCartoon = (long)NoCanPerCartoon,
                        PickupCartoonQty = PickupCartoonQty,
                        PickupQty = (PickupCartoonQty * (long)NoCanPerCartoon),
                        ProductionDate = DateTime.Parse(ProductionDate).Date,
                        BatchNo = BatchNo,
                        PickUpTransactionStatus = "InProgress",
                        UserIdAdd = UserID,
                        DateTimeAdd = DateTime.Now,
                        DeviceSerialNoAdd = DeviceSerialNo,
                        PalletCode = PalletCode_Pickup
                    };
                    DBContext.PickUpTransactions.Add(pickUpTransaction);
                }
                if (PalletCode_Pickup != PalletCode_Warehouse)
                {
                    if (GetPallet_WarehouseData.PickedupQtyFromPallet == GetPallet_WarehouseData.WarehouseReceivingQty) DBContext.PalletWips.Remove(GetPallet_WarehouseData);
                    else DBContext.PalletWips.Update(GetPallet_WarehouseData);
                }

                if (GetPurchaseRequisitionDetailsData.PickupQty == GetPurchaseRequisitionDetailsData.Qty) GetPurchaseRequisitionDetailsData.IsInProgress = false;

                DBContext.PurchaseRequisitionDetails.Update(GetPurchaseRequisitionDetailsData);

                var GetStock = DBContext.Stocks.FirstOrDefault(x => x.ProductCode == GetPallet_WarehouseData.ProductCode && x.ProductionDate.Value.Date == GetPallet_WarehouseData.ProductionDate.Value.Date && x.PlantCode == GetPallet_WarehouseData.PlantCode && x.StorageLocationCode == GetPallet_WarehouseData.StorageLocationCode);
                if (GetStock != null)
                {
                    var GetStockLocation = DBContext.StockLocations.FirstOrDefault(x => x.StockId == GetStock.StockId && x.ProductCode == GetPallet_WarehouseData.ProductCode && x.ProductionDate.Value.Date == GetPallet_WarehouseData.ProductionDate.Value.Date && x.PlantCode == GetPallet_WarehouseData.PlantCode && x.StorageLocationCode == GetPallet_WarehouseData.StorageLocationCode && x.LaneCode == GetPallet_WarehouseData.LaneCode);
                    if (GetStockLocation != null)
                    {
                        GetStockLocation.Qty -= (PickupCartoonQty * (long)NoCanPerCartoon);
                        if (GetStockLocation.Qty <= 0) DBContext.StockLocations.Remove(GetStockLocation);
                        else DBContext.StockLocations.Update(GetStockLocation);
                    }

                    GetStock.Qty -= (PickupCartoonQty * (long)NoCanPerCartoon);
                    if (GetStock.Qty <= 0) DBContext.Stocks.Remove(GetStock);
                    else DBContext.Stocks.Update(GetStock);
                }

                GetPurchaseRequisitionsData.StorageLocationCodeSource = StorageLocationCode;
                DBContext.SaveChanges();

                GetData = (from b in DBContext.PurchaseRequisitions
                           join d in DBContext.PurchaseRequisitionDetails on b.PurchaseRequisitionId equals d.PurchaseRequisitionId
                           where d.ProductCode == ProductCode
                           join pro in DBContext.Products on d.ProductCode equals pro.ProductCode
                           where b.PurchaseRequisitionId == PurchaseRequisitionId
                           select new PurchaseRequisition_Details
                           {
                               PurchaseRequisitionId = b.PurchaseRequisitionId,
                               PurchaseRequisitionNo = b.PurchaseRequisitionNo,
                               PurchaseRequisitionReleaseDate = b.PurchaseRequisitionReleaseDate,
                               PurchaseRequisitionQty = b.PurchaseRequisitionQty,
                               PlantCodeSource = b.PlantCodeSource,
                               StorageLocationCodeSource = b.StorageLocationCodeSource,
                               PlantCodeDestination = b.PlantCodeDestination,
                               StorageLocationCodeDestination = b.StorageLocationCodeDestination,
                               PurchaseRequisitionStatus = b.PurchaseRequisitionStatus,
                               NumberOfCartonsPerPallet = (int)((pro.NumeratorforConversionPal ?? 0) / (pro.DenominatorforConversionPal ?? 0)),
                               ProductCode = d.ProductCode,
                               ProductDesc = ((applang == "ar") ? pro.ProductDescAr : pro.ProductDesc),
                               Qty = d.Qty,
                               PickupQty = d.PickupQty,
                               Uom = d.Uom,
                               LineNumber = d.LineNumber,
                               LineStatus = d.LineStatus
                           }).Distinct().FirstOrDefault();

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetLaneDetails(long UserID, string DeviceSerialNo, string applang, string LaneCode)
        {
            ResponseStatus responseStatus = new();
            Lane LaneData = null;
            decimal AvailablePalletQty = 0;
            try
            {
                if (LaneCode == null || string.IsNullOrEmpty(LaneCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing lane code");
                    return new { responseStatus, LaneData, AvailablePalletQty };
                }
                LaneData = DBContext.Lanes.FirstOrDefault(x => x.LaneCode == LaneCode);
                if (LaneData == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المسار خاطئ" : "Lane code is wrong");
                    return new { responseStatus, LaneData, AvailablePalletQty };
                }
                AvailablePalletQty = LaneData.NoOfPallets ?? 0;

                var StockLocationData = DBContext.StockLocations.FirstOrDefault(x => x.LaneCode == LaneCode);
                if (StockLocationData != null)
                {
                    long TotalLaneQty = DBContext.StockLocations.Where(x => x.LaneCode == LaneCode).Sum(x => x.Qty).Value;

                    var ProductData = DBContext.Products.FirstOrDefault(x => x.ProductCode == StockLocationData.ProductCode);
                    if (ProductData != null)
                    {
                        decimal NoItemPerBox = ((ProductData.NumeratorforConversionPac ?? 0) / (ProductData.DenominatorforConversionPac ?? 0));
                        decimal NoBoxPerPallet = ((ProductData.NumeratorforConversionPal ?? 0) / (ProductData.DenominatorforConversionPal ?? 0));

                        AvailablePalletQty -= ((TotalLaneQty / NoItemPerBox) / NoBoxPerPallet);
                    }
                }

                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, LaneData, AvailablePalletQty };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPalletsForRouteCode(long UserID, string DeviceSerialNo, string applang, string RouteCode)
        {
            ResponseStatus responseStatus = new();
            List<RoutePalletParam> GetData = null;
            try
            {
                if (RouteCode == null || string.IsNullOrEmpty(RouteCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing route code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Routes.Any(x => x.RouteCode == RouteCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المسار خاطئ" : "Route code is wrong");
                    return new { responseStatus, GetData };
                }

                GetData = (from b in DBContext.PurchaseRequisitions
                           join rt in DBContext.Shipments on b.PlantCodeSource equals rt.PlantCodeDestination
                           join d in DBContext.PickUpTransactions on b.PurchaseRequisitionId equals d.PurchaseRequisitionId
                           join plant in DBContext.Plants on b.PlantCodeDestination equals plant.PlantCode
                           join product in DBContext.Products on d.ProductCode equals product.ProductCode
                           where rt.RouteCode == RouteCode && d.IsShipped != true && rt.IsShipped != true
                           select new RoutePalletParam
                           {
                               PurchaseRequisitionId = b.PurchaseRequisitionId,
                               PurchaseRequisitionNo = b.PurchaseRequisitionNo,
                               BatchNo = d.BatchNo,
                               PlantCodeDestination = b.PlantCodeDestination,
                               PlantDescDestination = plant.PlantDesc,
                               ProductCode = product.ProductCode,
                               ProductDesc = product.ProductDesc,
                               ProductionDate = d.ProductionDate,
                               PickupCartoonQty = d.PickupCartoonQty,
                               PalletCode = d.PalletCode,
                               IsShipped = d.IsShipped
                           }).Distinct().ToList();
                if (GetData != null && GetData.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPalletInfo(long UserID, string DeviceSerialNo, string applang, string PalletCode)
        {
            ResponseStatus responseStatus = new();
            RoutePalletParam GetData = null;
            try
            {
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المسار خاطئ" : "Route code is wrong");
                    return new { responseStatus, GetData };
                }

                GetData = (from b in DBContext.PurchaseRequisitions
                           join d in DBContext.PickUpTransactions on b.PurchaseRequisitionId equals d.PurchaseRequisitionId
                           join plant in DBContext.Plants on b.PlantCodeDestination equals plant.PlantCode
                           join product in DBContext.Products on d.ProductCode equals product.ProductCode
                           where d.PalletCode == PalletCode
                           select new RoutePalletParam
                           {
                               PurchaseRequisitionId = b.PurchaseRequisitionId,
                               PurchaseRequisitionNo = b.PurchaseRequisitionNo,
                               BatchNo = d.BatchNo,
                               PlantCodeDestination = b.PlantCodeDestination,
                               PlantDescDestination = plant.PlantDesc,
                               ProductCode = product.ProductCode,
                               ProductDesc = product.ProductDesc,
                               ProductionDate = d.ProductionDate,
                               PickupCartoonQty = d.PickupCartoonQty,
                               PalletCode = d.PalletCode,
                               IsShipped = d.IsShipped
                           }).Distinct().FirstOrDefault();
                if (GetData != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpPost]
        // This API used in mobile to close SAP batch (Inegration)  -> Need to ask mohammed if he use this API
        public object CloseBatch([FromBody] CloseBatchParam model)
        {
            ResponseStatus responseStatus = new();
            try
            {
                if (model.Qty == null || model.Qty <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الكمية" : "Missing quantity");
                    return new { responseStatus };
                }
                if (model.DateofManufacture == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ الصنع" : "Missing Date of Manufacture");
                    return new { responseStatus };
                }
                if (model.ProductCode == null || string.IsNullOrEmpty(model.ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus };
                }
                if (model.PlantCode == null || string.IsNullOrEmpty(model.PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus };
                }
                if (model.Storagelocation == null || string.IsNullOrEmpty(model.Storagelocation.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال مكان التخزين" : "Missing Storage location");
                    return new { responseStatus };
                }
                if (model.SapOrderId == null || string.IsNullOrEmpty(model.SapOrderId.ToString().Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال معرف SAP" : "Missing Sap Order Id");
                    return new { responseStatus };
                }
                if (model.Uom == null || string.IsNullOrEmpty(model.Uom.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الوحده الأساسيه للقياس" : "Missing Base Unit of Measure");
                    return new { responseStatus };
                }
                if (model.BatchNumber == null || string.IsNullOrEmpty(model.BatchNumber.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الباتش" : "Missing Batch Number");
                    return new { responseStatus };
                }

                if (!DBContext.StorageLocations.Any(x => x.StorageLocationCode == model.Storagelocation && x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود مكان التخزين " + model.Storagelocation + " خاطئ. أو لا يتعلق بالمصنع الخاص بأمر الشغل" : "Storage location code " + model.Storagelocation + " is wrong. Or is not related to the process order planet code");
                    return new { responseStatus };
                }
                if (!DBContext.Products.Any(x => x.ProductCode == model.ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus };
                }
                var productData = DBContext.Products.FirstOrDefault(x => x.ProductCode == model.ProductCode);

                bool IsCreatedOnSap = false;
                CloseBatchResponse Response = SAPIntegrationAPI.CloseBatchAPI(model.ProductCode, model.SapOrderId.ToString(), model.Qty.Value.ToString(), model.BatchNumber, model.DateofManufacture.Value.ToString("yyyy-MM-dd"), model.Uom, DateTime.Now.ToString("yyyy-MM-dd"), model.PlantCode, model.Storagelocation);
                if (Response != null && Response.messageType == "S")
                {
                    IsCreatedOnSap = true;
                }
                CloseBatch entity = new()
                {
                    Uom = model.Uom,
                    PlantCode = model.PlantCode,
                    Storagelocation = model.Storagelocation,
                    BatchNumber = model.BatchNumber,
                    ProductCode = model.ProductCode,
                    DateofManufacture = model.DateofManufacture,
                    SapOrderId = model.SapOrderId,
                    Qty = model.Qty,
                    PostingDateintheDocument = DateTime.Now,
                    UserIdAdd = model.UserID,
                    DateTimeAdd = DateTime.Now,
                    IsCreatedOnSap = IsCreatedOnSap,
                    MessageCode = Response.messageCode,
                    MessageText = Response.messageText,
                    NumberofMaterialDocument = Response.MBLNR,
                    MessageType = Response.messageType,
                    MaterialDocumentYear = Response.MJAHR,
                    Message = Response.message
                };
                DBContext.CloseBatchs.Add(entity);
                DBContext.SaveChanges();


                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;

                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        [Route("[action]")]
        [HttpGet]
        // This API used in mobile to get shipment list.  (We can edit this api to get all shipment from SAP then insert it like (SapShipment_Insert))
        public object GetSAPShipmentList(long UserID, string DeviceSerialNo, string PlantCode, string PlannedDate, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ShipmentParams> GetList = new();
            List<SapShipmentRequest_Response> SapShipment = new();
            List<SapShipmentRequest_Response> ErrorLog = new();
            try
            {
                if (PlantCode == null || string.IsNullOrEmpty(PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing Plant Code");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع خاطئ" : "Wrong plant code");
                    return new { responseStatus, GetList };
                }
                var GetShipmentTypes = DBContext.ShipmentTypePlants.Where(x => x.PlantCode == PlantCode).Select(x => x.ShipmentTypeCode).ToList();


                SapShipment = SAPIntegrationAPI.GetSapShipment(GetShipmentTypes, PlannedDate, PlantCode);
                if (SapShipment != null && SapShipment.Count > 0)
                {
                    foreach (var item in SapShipment)
                    {
                        try
                        {
                            if (item.TKNUM == null || string.IsNullOrEmpty(item.TKNUM.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال رقم الشحنه" : "Missing shipment no");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.SHTYP == null || string.IsNullOrEmpty(item.SHTYP.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود نوع الشحنه" : "Missing shipment type code");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (item.ROUTE == null || string.IsNullOrEmpty(item.ROUTE.Trim()))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing route code");
                                ErrorLog.Add(item);
                                continue;
                            }
                            //if (item.PlantCode_Destination == null || string.IsNullOrEmpty(item.PlantCode_Destination.Trim()))
                            //{
                            //    item.ErrorMsg = ((applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                            //    ErrorLog.Add(item);
                            //    continue;
                            //}

                            //if (!DBContext.Plants.Any(x => x.PlantCode == item.PlantCode_Destination))
                            //{
                            //    item.ErrorMsg = ((applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                            //    ErrorLog.Add(item);
                            //    continue;
                            //}

                            if (!DBContext.Routes.Any(x => x.RouteCode == item.ROUTE))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "كود المسار خاطئ" : "Route code is wrong");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (!DBContext.ShipmentTypePlants.Any(x => x.ShipmentTypeCode == item.SHTYP))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "كود نوع الشحنه خاطئ" : "Shipment Type Code is wrong");
                                ErrorLog.Add(item);
                                continue;
                            }
                            if (!DBContext.ShipmentTypePlants.Any(x => x.ShipmentTypeCode == item.SHTYP && x.PlantCode == PlantCode))
                            {
                                item.ErrorMsg = ((applang == "ar") ? "كود نوع الشحنه خاطئ او لا ينتمي للمصنع التي تم اختياره " : "Shipment type Code is wrong or not related to the selected plant");
                                ErrorLog.Add(item);
                                continue;
                            }

                            if (DBContext.Shipments.Any(x => x.ShipmentNo == item.TKNUM))
                            {
                                var entity = DBContext.Shipments.FirstOrDefault(x => x.ShipmentNo == item.TKNUM);

                                entity.RouteCode = item.ROUTE;
                                entity.PlantCodeDestination = PlantCode;
                                entity.ShipmentTypeCode = item.SHTYP;

                                DBContext.Shipments.Update(entity);
                                DBContext.SaveChanges();
                            }
                            else
                            {
                                Shipment entity = new()
                                {
                                    ShipmentNo = item.TKNUM,
                                    SapshipmentId = 1,
                                    RouteCode = item.ROUTE,
                                    PlantCodeDestination = PlantCode,
                                    ShipmentTypeCode = item.SHTYP
                                };
                                DBContext.Shipments.Add(entity);
                                DBContext.SaveChanges();
                            }

                        }
                        catch (Exception ex)
                        {
                            item.ErrorMsg = "Exception Error: " + ex.Message;
                            ErrorLog.Add(item);
                            continue;
                        }
                    }
                }


                GetList = (from b in DBContext.Shipments
                           join r in DBContext.Routes on b.RouteCode equals r.RouteCode
                           where b.PlantCodeDestination == PlantCode && GetShipmentTypes.Contains(b.ShipmentTypeCode) && b.IsShipped != true
                           select new ShipmentParams
                           {
                               ShipmentNo = b.ShipmentNo,
                               ShipmentTypeCode = b.ShipmentTypeCode,
                               PlantCodeDestination = b.PlantCodeDestination,
                               RouteCode = b.RouteCode,
                               RouteDesc = r.RouteDesc,
                               TruckCapacity = b.TruckCapacity,
                               VendorNo = b.VendorNo,
                               VendorName = b.VendorName,
                               RouteId = r.RouteId
                           }
               ).Distinct().ToList().Select(x =>
               {
                   var GetRouteDetails = DBContext.RouteDetails.Where(c => c.RouteId == x.RouteId).Select(g => g.DestinationPoint).ToList();
                   return new ShipmentParams
                   {
                       ShipmentNo = x.ShipmentNo,
                       ShipmentTypeCode = x.ShipmentTypeCode,
                       PlantCodeDestination = x.PlantCodeDestination,
                       RouteCode = x.RouteCode,
                       RouteDesc = x.RouteDesc,
                       RouteDestinations = GetRouteDetails,
                       TruckCapacity = x.TruckCapacity,
                       VendorNo = x.VendorNo,
                       VendorName = x.VendorName,
                       RouteId = x.RouteId
                   };
               }).ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList, ErrorLog };
        }
        //This API used in mobile to post shipment to SAP (Integration).
        [Route("[action]")]
        [HttpPost]
        public object PostShipment([FromBody] PostShipmentParam model)
        {
            ResponseStatus responseStatus = new();
            List<SapPostShipmentRequest_Response> Sap_PostShipment = new();
            try
            {
                if (model.ShipmentNo == null || string.IsNullOrEmpty(model.ShipmentNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم الشحن" : "Missing Shipment No");
                    return new { responseStatus };
                }
                if (model.RouteCode == null || string.IsNullOrEmpty(model.RouteCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المسار" : "Missing Route code");
                    return new { responseStatus };
                }
                if (model.Pallets == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال الباليت" : "Missing Pallets data");
                    return new { responseStatus };
                }
                if (model.TruckPlateNo == null || string.IsNullOrEmpty(model.TruckPlateNo.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال رقم السياره" : "Missing Truck Plate No");
                    return new { responseStatus };
                }
                if (model.DriverName == null || string.IsNullOrEmpty(model.DriverName.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال اسم السائق" : "Missing Driver Name");
                    return new { responseStatus };
                }
                SapPostShipmentRequestList_Request sapPostShipmentRequestList_Request = new SapPostShipmentRequestList_Request();

                foreach (var item in model.Pallets)
                {
                    var GetPalletData = DBContext.PickUpTransactions.FirstOrDefault(x => x.PalletCode == item.PalletCode);
                    GetPalletData.IsShipped = true;
                    DBContext.PickUpTransactions.Update(GetPalletData);

                    var GetPurchaseRequisitionNo = (from x in DBContext.PickUpTransactions
                                                    join pr in DBContext.PurchaseRequisitions on x.PurchaseRequisitionId equals pr.PurchaseRequisitionId
                                                    where x.PalletCode == item.PalletCode
                                                    select pr).FirstOrDefault();

                    var GetPurchaseRequisitionDetails = (from x in DBContext.PickUpTransactions
                                                         join pr in DBContext.PurchaseRequisitionDetails on new { x.PurchaseRequisitionId, x.ProductCode } equals new { pr.PurchaseRequisitionId, pr.ProductCode }
                                                         where x.PalletCode == item.PalletCode
                                                         select pr).FirstOrDefault();

                    SapPostShipmentRequest_Request sapPostShipmentRequest_Request = new SapPostShipmentRequest_Request()
                    {
                        TKNUM = model.ShipmentNo,
                        BANFN = GetPurchaseRequisitionNo.PurchaseRequisitionNo,
                        BNFPO = GetPurchaseRequisitionDetails.LineNumber ?? 0,
                        CHARG = GetPalletData.BatchNo,
                        MATNR = GetPalletData.ProductCode,
                        MENGE = GetPurchaseRequisitionDetails.Qty ?? 0,
                        ROUTE = model.RouteCode,
                        SIGNI = model.TruckPlateNo,
                        EXTI1 = model.DriverName,
                        DPREG = DateTime.Now.ToString("ddMMyyyy")
                    };

                    sapPostShipmentRequestList_Request.GetList.Add(sapPostShipmentRequest_Request);

                }

                var GetShipmentData = DBContext.Shipments.FirstOrDefault(x => x.ShipmentNo == model.ShipmentNo);
                GetShipmentData.IsShipped = true;
                DBContext.Shipments.Update(GetShipmentData);
                DBContext.SaveChanges();

                Sap_PostShipment = SAPIntegrationAPI.SapPostShipment(sapPostShipmentRequestList_Request);


                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");


            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, Sap_PostShipment };
        }

        [Route("[action]")]
        [HttpGet]
        public object ScanPallet_CheckIn(long UserID, string DeviceSerialNo, string applang, string PalletCode, string LocationCode)
        {
            ResponseStatus responseStatus = new();
            PalletCheckInOutTransaction palletDetails = new();
            try
            {

                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, palletDetails };
                }

                if (LocationCode == null || string.IsNullOrEmpty(LocationCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود موقع الباليت" : "Missing pallet location code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletLocations.Any(x => x.PalletLocationCode == LocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود موقع الباليت خاطئ" : "Pallet location code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus, palletDetails };
                }
                if (DBContext.PalletCheckInOutTransactions.Any(x => x.PalletCode == PalletCode && x.UserIdCheckOut == null && x.LocationCode == LocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم تسجيل دخول بالفعل الباليت فى هذا الموقع" : "The pallet has already been checked in at this location");
                    return new { responseStatus, palletDetails };
                }

                var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                PalletCheckInOutTransaction palletCheckInOutTransactions = new()
                {
                    LocationCode = LocationCode,

                    BatchNo = PalletData.BatchNo,
                    PalletCode = PalletData.PalletCode,
                    PlantCode = PalletData.PlantCode,
                    ProductCode = PalletData.ProductCode,
                    ProductionDate = PalletData.ProductionDate,
                    ProductionLineId = PalletData.ProductionLineId,
                    ProductionOrderId = PalletData.ProductionOrderId,
                    SaporderId = PalletData.SaporderId,

                    UserIdCheckIn = UserID,
                    DateTimeCheckIn = DateTime.Now,
                    DeviceSerialNoCheckIn = DeviceSerialNo
                };

                DBContext.PalletCheckInOutTransactions.Add(palletCheckInOutTransactions);

                DBContext.SaveChanges();


                palletDetails = DBContext.PalletCheckInOutTransactions.FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object ScanPallet_CheckOut(long UserID, string DeviceSerialNo, string applang, string PalletCode, string LocationCode)
        {
            ResponseStatus responseStatus = new();
            PalletCheckInOutTransaction palletDetails = new();
            try
            {
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, palletDetails };
                }

                if (LocationCode == null || string.IsNullOrEmpty(LocationCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود موقع الباليت" : "Missing pallet location code");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletLocations.Any(x => x.PalletLocationCode == LocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود موقع الباليت خاطئ" : "Pallet location code is wrong");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus, palletDetails };
                }
                if (!DBContext.PalletCheckInOutTransactions.Any(x => x.PalletCode == PalletCode && x.UserIdCheckIn != null && x.UserIdCheckOut == null && x.LocationCode == LocationCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد باليت في هذا الموقع تم تسجيل دخولها" : "No pallet in this location has been checked in");
                    return new { responseStatus, palletDetails };
                }
                var palletCheckInOutTransactions = DBContext.PalletCheckInOutTransactions.FirstOrDefault(x => x.PalletCode == PalletCode && x.UserIdCheckIn != null && x.UserIdCheckOut == null && x.LocationCode == LocationCode);

                palletCheckInOutTransactions.UserIdCheckOut = UserID;
                palletCheckInOutTransactions.DateTimeCheckOut = DateTime.Now;
                palletCheckInOutTransactions.DeviceSerialNoCheckOut = DeviceSerialNo;

                DBContext.PalletCheckInOutTransactions.Update(palletCheckInOutTransactions);

                DBContext.SaveChanges();


                palletDetails = DBContext.PalletCheckInOutTransactions.FirstOrDefault();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Data saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetWarehouseBatches(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<BatchesListParam> batchesList = new();
            try
            {
                batchesList = (from b in DBContext.PalletWips
                               select new BatchesListParam
                               {
                                   BatchNo = b.BatchNo,
                                   ProductionOrderId = b.ProductionOrderId
                               }).Distinct().ToList();

                if (batchesList != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, batchesList };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetWarehouseBatchPallets(long UserID, string DeviceSerialNo, string applang, long ProductionOrderId, string BatchNo)
        {
            ResponseStatus responseStatus = new();
            List<PalletDetails2Param> palletDetails = new();
            try
            {
                palletDetails = (from b in DBContext.PalletWips
                                 join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                                 join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                                 join d in DBContext.ProductionOrderDetails on new
                                 {
                                     ProductionOrderId = b.ProductionOrderId.Value,
                                     BatchNo = b.BatchNo,
                                     ProductionLineId = b.ProductionLineId.Value
                                 } equals new
                                 {
                                     d.ProductionOrderId,
                                     d.BatchNo,
                                     ProductionLineId = d.ProductionLineId.Value
                                 }
                                 join p in DBContext.Products on b.ProductCode equals p.ProductCode
                                 join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                                 join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                                 join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                                 where b.ProductionOrderId == ProductionOrderId
                                  && b.BatchNo == BatchNo && d.IsClosedBatch != true
                                 select new PalletDetails2Param
                                 {
                                     PalletCode = b.PalletCode,
                                     PlantId = x.PlantId,
                                     PlantCode = x.PlantCode,
                                     PlantDesc = x.PlantDesc,
                                     OrderTypeId = o.OrderTypeId,
                                     OrderTypeCode = o.OrderTypeCode,
                                     OrderTypeDesc = o.OrderTypeDesc,
                                     ProductId = p.ProductId,
                                     ProductCode = p.ProductCode,
                                     ProductDesc = p.ProductDesc,
                                     ProductionOrderId = l.ProductionOrderId,
                                     SapOrderId = l.SapOrderId.Value,
                                     ProductionOrderQty = l.Qty,
                                     ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     OrderDate = l.OrderDate,
                                     BatchNo = r.BatchNo,
                                     PalletQty = r.Qty,
                                     PalletCartoonQty = Math.Round((r.Qty / ((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))).Value, 2),
                                     IsWarehouseLocation = b.IsWarehouseLocation,
                                     IsWarehouseReceived = b.IsWarehouseReceived,
                                     StorageLocationCode = b.StorageLocationCode,
                                     WarehouseReceivingQty = b.WarehouseReceivingQty,
                                     WarehouseReceivingCartoonQty = ((b.WarehouseReceivingQty > 0) ? Math.Round((b.WarehouseReceivingQty / ((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))).Value, 2) : 0),
                                     LaneCode = b.LaneCode,
                                     ProductionLineId = ln.ProductionLineId,
                                     ProductionLineCode = ln.ProductionLineCode,
                                     ProductionLineDesc = ln.ProductionLineDesc,
                                     BatchQty = d.Qty,
                                     BatchQtyCartoon = (r.BatchNo != null) ? (d.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (d.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                                     ReceivedQty = b.ReceivingQty,
                                     ReceivedQtyCartoon = (r.BatchNo != null) ? (b.ReceivingQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (b.ReceivingQty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))),
                                     WarehouseReceivingPackage = r.WarehouseReceivingPackage,
                                     WarehouseReceivingCartoonReceivedQty = r.WarehouseReceivingCartoonReceivedQty
                                 }).Distinct().ToList();

                if (palletDetails != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, palletDetails };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPalletList_ForPrint(long UserID, string DeviceSerialNo, string applang, string PalletCode, long? PalletCodeQty, bool? IsQty)
        {
            ResponseStatus responseStatus = new();
            List<string> GetData = new();
            try
            {
                if (IsQty == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = "Missing IsQty";
                    return new { responseStatus, GetData };
                }

                if (IsQty == true)
                {
                    if (PalletCodeQty == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = "Missing PalletCodeQty";
                        return new { responseStatus, GetData };
                    }
                    //string Prefix = DBContext.PalletSettings.FirstOrDefault().PalletCode;
                    long StartIndex = DBContext.PalletSettings.FirstOrDefault().AutoNo.GetValueOrDefault(0) + 1;
                    long EndIndex = DBContext.PalletSettings.FirstOrDefault().AutoNo.GetValueOrDefault(0) + PalletCodeQty.GetValueOrDefault(0);

                    for (long i = StartIndex; i <= EndIndex; i++)
                    {
                        if (!DBContext.Pallets.Any(x => x.PalletCode == i.ToString("000000000000")))
                        {
                            Pallet pallet = new Pallet
                            {
                                PalletCode = i.ToString("000000000000")
                            };
                            DBContext.Pallets.Add(pallet);
                        }
                        //if (!DBContext.Pallets.Any(x => x.PalletCode == Prefix + i.ToString("0000000")))
                        //{
                        //    Pallet pallet = new Pallet
                        //    {
                        //        PalletCode = Prefix + i.ToString("0000000")
                        //    };
                        //    DBContext.Pallets.Add(pallet);
                        //}
                    }
                    PalletSetting palletSetting = DBContext.PalletSettings.FirstOrDefault();
                    palletSetting.AutoNo += PalletCodeQty;
                    DBContext.PalletSettings.Update(palletSetting);

                    DBContext.SaveChanges();

                    for (long i = StartIndex; i <= EndIndex; i++)
                    {
                        GetData.Add(i.ToString("000000000000"));
                    }

                }
                else
                {
                    if (PalletCode == null)
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = "Missing PalletCode";
                        return new { responseStatus, GetData };
                    }
                    if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                    {
                        Pallet pallet = new Pallet
                        {
                            PalletCode = PalletCode
                        };
                        DBContext.Pallets.Add(pallet);
                        DBContext.SaveChanges();
                    }

                    GetData = (from b in DBContext.Pallets
                               where b.PalletCode == PalletCode
                               select b.PalletCode).ToList();
                }

                if (GetData != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object GetPallet_Incubation(long UserID, string DeviceSerialNo, string applang, string PalletCode)
        {
            ResponseStatus responseStatus = new();
            PalletDetailsParam GetData = new();
            try
            {

                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus, GetData };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus, GetData };
                }


                GetData = (from b in DBContext.PalletWips
                           join l in DBContext.ProductionOrders on b.ProductionOrderId equals l.ProductionOrderId
                           join r in DBContext.ProductionOrderReceivings on new { b.PalletCode, b.ProductionOrderId, b.BatchNo, b.ProductionLineId } equals new { r.PalletCode, r.ProductionOrderId, r.BatchNo, r.ProductionLineId }
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on l.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on l.OrderTypeCode equals o.OrderTypeCode
                           join ln in DBContext.ProductionLines on b.ProductionLineId equals ln.ProductionLineId
                           where r.PalletCode == PalletCode
                           select new PalletDetailsParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,
                               NumeratorforConversionPac = p.NumeratorforConversionPac,
                               NumeratorforConversionPal = p.NumeratorforConversionPal,
                               DenominatorforConversionPal = p.DenominatorforConversionPal,
                               DenominatorforConversionPac = p.DenominatorforConversionPac,
                               ProductionOrderId = l.ProductionOrderId,
                               SapOrderId = l.SapOrderId.Value,
                               ProductionOrderQty = l.Qty,
                               ProductionOrderQtyCartoon = (r.BatchNo != null) ? (l.Qty / (long)((r.NumeratorforConversionPac ?? 0) / (r.DenominatorforConversionPac ?? 0))) : (l.Qty / (long)((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               OrderDate = l.OrderDate,
                               BatchNo = r.BatchNo,
                               ProductionDate = r.ProductionDate,
                               PalletQty = r.Qty,
                               PalletCartoonQty = (r.Qty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               IsWarehouseLocation = b.IsWarehouseLocation,
                               ProductionQtyCheckIn = b.ProductionQtyCheckIn,
                               ProductionQtyCheckOut = b.ProductionQtyCheckOut,
                               IsWarehouseReceived = b.IsWarehouseReceived,
                               StorageLocationCode = b.StorageLocationCode,
                               WarehouseReceivingQty = b.WarehouseReceivingQty,
                               WarehouseReceivingCartoonQty = (decimal)(b.WarehouseReceivingQty / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               LaneCode = b.LaneCode,
                               PalletCode = r.PalletCode,
                               PickedupQtyFromPallet = b.PickedupQtyFromPallet,
                               PickedupCartoonQtyFromPallet = (b.PickedupQtyFromPallet / ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))),
                               NoCanPerCartoon = ((p.NumeratorforConversionPac ?? 0) / (p.DenominatorforConversionPac ?? 0))
                           }).FirstOrDefault();

                if (GetData != null)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;
                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetData };
        }
        [Route("[action]")]
        [HttpGet]
        public object Incubation_PalletCheckInOut(long UserID, string DeviceSerialNo, string applang, string PalletCode, long? LocationId, DateTime? TransDate)
        {
            ResponseStatus responseStatus = new();
            try
            {
                if (PalletCode == null || string.IsNullOrEmpty(PalletCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الباليت" : "Missing pallet code");
                    return new { responseStatus };
                }
                if (!DBContext.Pallets.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الباليت خاطئ" : "Pallet code is wrong");
                    return new { responseStatus };
                }
                if (LocationId == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود الموقع" : "Missing Location Id");
                    return new { responseStatus };
                }
                if (TransDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال التاريخ" : "Missing Trans Date");
                    return new { responseStatus };
                }
                if (!DBContext.Locations.Any(x => x.LocationId == LocationId))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود الموقع خاطئ" : "Location Id is wrong");
                    return new { responseStatus };
                }
                if (!DBContext.PalletWips.Any(x => x.PalletCode == PalletCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم تحميل الباليت" : "Pallet is not loaded");
                    return new { responseStatus };
                }

                //if (DBContext.PalletIncubations.Any(x => x.PalletCode == PalletCode && x.LocationId == LocationId))
                if (DBContext.PalletIncubations.Any(x => x.PalletCode == PalletCode))
                {
                    var palletIncubation = DBContext.PalletIncubations.FirstOrDefault(x => x.PalletCode == PalletCode);
                    if (palletIncubation != null)
                    {
                        DBContext.PalletIncubations.Remove(palletIncubation);

                        var palletIncubationsHistory1 = DBContext.PalletIncubationsHistories.FirstOrDefault(x => x.PalletCode == PalletCode && x.PalletIncubationsId == palletIncubation.PalletIncubationsId);

                        if (palletIncubationsHistory1 != null)
                        {
                            palletIncubationsHistory1.LocationId = LocationId;
                            palletIncubationsHistory1.DateTimeCheckOut = DateTime.Now;
                            palletIncubationsHistory1.UserCheckOut = UserID;
                            palletIncubationsHistory1.DeviceSerialNoCheckOut = DeviceSerialNo;
                            palletIncubationsHistory1.TransDateCheckOut = TransDate;
                            DBContext.PalletIncubationsHistories.Update(palletIncubationsHistory1);
                        }
                        DBContext.SaveChanges();
                        responseStatus.StatusCode = 200;
                        responseStatus.IsSuccess = true;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم تسجيل خروج الباليت بنجاح" : "Pallet checked out successfully");
                    }
                }
                else
                {

                    var PalletData = DBContext.PalletWips.FirstOrDefault(x => x.PalletCode == PalletCode);
                    if (PalletData != null)
                    {
                        var product = DBContext.Products.FirstOrDefault(x => x.ProductCode == PalletData.ProductCode);
                        if (!DBContext.ProductMaps.Any(x => x.ProductId == product.ProductId && x.LocationId == LocationId))
                        {
                            responseStatus.StatusCode = 401;
                            responseStatus.IsSuccess = false;
                            string statusMessage = applang == "ar"
                                ? "هذا المكان لا ينتمي للصنف الموجود على الباليت"
                                : "This location is not assigned to the specified product that located in the pallet";

                            responseStatus.StatusMessage = statusMessage;
                            return new { responseStatus };
                        }
                        PalletIncubation palletIncubation = new PalletIncubation()
                        {
                            LocationId = LocationId,
                            PalletCode = PalletCode
                        };
                        DBContext.PalletIncubations.Add(palletIncubation);
                        DBContext.SaveChanges();

                        PalletIncubationsHistory palletIncubationsHistory = new PalletIncubationsHistory()
                        {
                            LocationId = LocationId,
                            PalletCode = PalletCode,
                            BatchNo = PalletData.BatchNo,
                            DeviceSerialNoCheckIn = DeviceSerialNo,
                            PalletQty = PalletData.ReceivingQty,
                            PlantCode = PalletData.PlantCode,
                            ProductCode = PalletData.ProductCode,
                            ProductionDate = PalletData.ProductionDate,
                            ProductionLineId = PalletData.ProductionLineId,
                            SaporderId = PalletData.SaporderId,
                            ProductionOrderId = PalletData.ProductionOrderId,
                            UserCheckIn = UserID,
                            TransDateCheckIn = TransDate,
                            PalletIncubationsId = palletIncubation.PalletIncubationsId
                        };
                        DBContext.PalletIncubationsHistories.Add(palletIncubationsHistory);
                        DBContext.SaveChanges();

                        responseStatus.StatusCode = 200;
                        responseStatus.IsSuccess = true;
                        responseStatus.StatusMessage = ((applang == "ar") ? "تم تسجيل دخول الباليت بنجاح" : "Pallet checked in successfully");
                    }

                }


            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus };
        }
        #endregion
        #region Deprecated APIs
        [Route("[action]")]
        [HttpPost]
        //Not used till now and it call the SAP integration api (GetSapPurchaseRequest)
        public object GetSap_PurchaseRequest([FromBody] GetSap_PurchaseRequestParam model)
        {
            ResponseStatus responseStatus = new();

            List<SapPurchaseRequest_Response> SapPurchaseRequest = new();
            try
            {
                if (model.PurchaseRequisitionReleaseDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال تاريخ إصدار طلب الشراء" : "Missing Purchase Requisition Release Date");
                    return new { responseStatus, SapPurchaseRequest };
                }
                if (model.IssuingPlantCode == null || string.IsNullOrEmpty(model.IssuingPlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع المصدر" : "Missing issuing plant code");
                    return new { responseStatus, SapPurchaseRequest };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == model.IssuingPlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع المصدر خاطئ" : "Issuing plant code is wrong");
                    return new { responseStatus, SapPurchaseRequest };
                }
                if (model.PlantCode != null && !string.IsNullOrEmpty(model.PlantCode.Trim()))
                {
                    if (!DBContext.Plants.Any(x => x.PlantCode == model.PlantCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                        return new { responseStatus, SapPurchaseRequest };
                    }
                }
                if (model.ProductCode != null && !string.IsNullOrEmpty(model.ProductCode.Trim()))
                {
                    if (!DBContext.Products.Any(x => x.ProductCode == model.ProductCode))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                        return new { responseStatus, SapPurchaseRequest };
                    }
                }
                SapPurchaseRequest = SAPIntegrationAPI.GetSapPurchaseRequest(model.PurchaseRequisitionReleaseDate, model.IssuingPlantCode, model.ProductCode, model.PlantCode);
                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;

                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, SapPurchaseRequest };
        }
        [Route("[action]")]
        [HttpPost]
        //Not used till now and it call the SAP integration api (GetSapShipment)
        public object GetSap_Shipment([FromBody] GetSap_ShipmentParam model)
        {
            ResponseStatus responseStatus = new();

            List<SapShipmentRequest_Response> SapShipment = new();
            try
            {
                if (model.PlannedDate == null)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال التاريخ المحدد" : "Missing planned date");
                    return new { responseStatus, SapShipment };
                }
                if (model.ShipmentTypeCode == null || model.ShipmentTypeCode.Count <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود نوع الشحنة" : "Missing shipment type code");
                    return new { responseStatus, SapShipment };
                }
                foreach (var item in model.ShipmentTypeCode)
                {
                    if (!DBContext.ShipmentTypes.Any(x => x.ShipmentTypeCode == item))
                    {
                        responseStatus.StatusCode = 401;
                        responseStatus.IsSuccess = false;
                        responseStatus.StatusMessage = ((model.applang == "ar") ? "كود نوع الشحنة خاطئ" : "Shipment type code is wrong");
                        return new { responseStatus, SapShipment };
                    }
                }

                if (model.PlantCode == null || string.IsNullOrEmpty(model.PlantCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "لم يتم إدخال كود المصنع" : "Missing plant code");
                    return new { responseStatus, SapShipment };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == model.PlantCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((model.applang == "ar") ? "كود المصنع خاطئ" : "Plant code is wrong");
                    return new { responseStatus, SapShipment };
                }

                SapShipment = SAPIntegrationAPI.GetSapShipment(model.ShipmentTypeCode, model.PlannedDate, model.PlantCode);
                responseStatus.StatusCode = 200;
                responseStatus.IsSuccess = true;

                responseStatus.StatusMessage = ((model.applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");

            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((model.applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");
                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, SapShipment };
        }
        [Route("[action]")]
        [HttpGet]
        //I asked Mohamed about this API and he told me it's not in use
        public object ProductionOrdersSapGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<ProcessOrdersParam> GetList = new();
            try
            {
                GetList = (from b in DBContext.ProductionOrders
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join x in DBContext.Plants on b.PlantCode equals x.PlantCode
                           join o in DBContext.OrderTypes on b.OrderTypeCode equals o.OrderTypeCode
                           select new ProcessOrdersParam
                           {
                               PlantId = x.PlantId,
                               PlantCode = x.PlantCode,
                               PlantDesc = x.PlantDesc,
                               OrderTypeId = o.OrderTypeId,
                               OrderTypeCode = o.OrderTypeCode,
                               OrderTypeDesc = o.OrderTypeDesc,
                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,

                               ProductionOrderId = b.ProductionOrderId,
                               SapOrderId = b.SapOrderId,
                               Qty = b.Qty,
                               OrderDate = b.OrderDate,
                               IsMobile = b.IsMobile
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        //I asked Mohamed about this API and he told me it's not in use
        public object PurchaseRequestGetList(long UserID, string DeviceSerialNo, string applang)
        {
            ResponseStatus responseStatus = new();
            List<PurchaseRequestParam> GetList = new();
            try
            {
                GetList = (from b in DBContext.PurchaseRequests
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join xIssue in DBContext.Plants on b.PlantCodeIssue equals xIssue.PlantCode
                           join xDestination in DBContext.Plants on b.PlantCodeDestination equals xDestination.PlantCode
                           select new PurchaseRequestParam
                           {
                               PlantId_Issue = xIssue.PlantId,
                               PlantCode_Issue = xIssue.PlantCode,
                               PlantDesc_Issue = xIssue.PlantDesc,

                               PlantId_Destination = xDestination.PlantId,
                               PlantCode_Destination = xDestination.PlantCode,
                               PlantDesc_Destination = xDestination.PlantDesc,

                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,

                               Prid = b.Prid,
                               Prno = b.Prno,
                               Qty = b.Qty,
                               ShipmentDate = b.ShipmentDate
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم إرسال البيانات بنجاح" : "Data sent successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        [Route("[action]")]
        [HttpGet]
        //I asked Mohamed about this API and he told me it's not in use
        public object PurchaseRequestCreate(long UserID, string DeviceSerialNo, string Prno, long? Qty, string ShipmentDate, string ProductCode, string PlantCodeIssue, string PlantCodeDestination, string applang)
        {
            ResponseStatus responseStatus = new();
            List<PurchaseRequestParam> GetList = new();
            try
            {
                if (Prno == null || string.IsNullOrEmpty(Prno.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال رقم طلب الشراء" : "Missing purchase request number");
                    return new { responseStatus, GetList };
                }
                if (Qty == null || Qty <= 0)
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال الكمية" : "Missing quantity");
                    return new { responseStatus, GetList };
                }
                if (ShipmentDate == null || string.IsNullOrEmpty(ShipmentDate.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال تاريخ الشحن" : "Missing shipment date");
                    return new { responseStatus, GetList };
                }
                if (ProductCode == null || string.IsNullOrEmpty(ProductCode.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المنتج" : "Missing product code");
                    return new { responseStatus, GetList };
                }
                if (PlantCodeIssue == null || string.IsNullOrEmpty(PlantCodeIssue.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع الوارد منه" : "Missing plant code issue");
                    return new { responseStatus, GetList };
                }
                if (PlantCodeDestination == null || string.IsNullOrEmpty(PlantCodeDestination.Trim()))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "لم يتم إدخال كود المصنع الصادر اليه" : "Missing plant code destination");
                    return new { responseStatus, GetList };
                }

                if (!DBContext.Products.Any(x => x.ProductCode == ProductCode))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المنتج خاطئ" : "Product code is wrong");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCodeIssue))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع الوارد منه خاطئ" : "Plant code issue is wrong");
                    return new { responseStatus, GetList };
                }
                if (!DBContext.Plants.Any(x => x.PlantCode == PlantCodeDestination))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "كود المصنع الصادر اليه خاطئ" : "Plant code destination is wrong");
                    return new { responseStatus, GetList };
                }

                if (DBContext.PurchaseRequests.Any(x => x.Prno == Prno))
                {
                    responseStatus.StatusCode = 401;
                    responseStatus.IsSuccess = false;
                    responseStatus.StatusMessage = ((applang == "ar") ? "رقم طلب الشراء موجود مسبقا" : "Purchase request number is already exists");
                    return new { responseStatus, GetList };
                }

                PurchaseRequest entity = new()
                {
                    PlantCodeIssue = PlantCodeIssue,
                    ProductCode = ProductCode,
                    PlantCodeDestination = PlantCodeDestination,
                    ShipmentDate = ShipmentDate,
                    Prno = Prno,
                    Qty = Qty
                };
                DBContext.PurchaseRequests.Add(entity);
                DBContext.SaveChanges();

                GetList = (from b in DBContext.PurchaseRequests
                           join p in DBContext.Products on b.ProductCode equals p.ProductCode
                           join xIssue in DBContext.Plants on b.PlantCodeIssue equals xIssue.PlantCode
                           join xDestination in DBContext.Plants on b.PlantCodeDestination equals xDestination.PlantCode
                           select new PurchaseRequestParam
                           {
                               PlantId_Issue = xIssue.PlantId,
                               PlantCode_Issue = xIssue.PlantCode,
                               PlantDesc_Issue = xIssue.PlantDesc,

                               PlantId_Destination = xDestination.PlantId,
                               PlantCode_Destination = xDestination.PlantCode,
                               PlantDesc_Destination = xDestination.PlantDesc,

                               ProductId = p.ProductId,
                               ProductCode = p.ProductCode,
                               ProductDesc = p.ProductDesc,

                               Prid = b.Prid,
                               Prno = b.Prno,
                               Qty = b.Qty,
                               ShipmentDate = b.ShipmentDate
                           }).Distinct().ToList();
                if (GetList != null && GetList.Count > 0)
                {
                    responseStatus.StatusCode = 200;
                    responseStatus.IsSuccess = true;

                    responseStatus.StatusMessage = ((applang == "ar") ? "تم حفظ البيانات بنجاح" : "Saved successfully");
                }
                else
                {
                    responseStatus.StatusCode = 400;
                    responseStatus.IsSuccess = false;

                    responseStatus.StatusMessage = ((applang == "ar") ? "لا توجد بيانات" : "No data found");
                }
            }
            catch (Exception ex)
            {
                responseStatus.StatusCode = 500;
                responseStatus.IsSuccess = false;
                responseStatus.StatusMessage = ((applang == "ar") ? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator");

                responseStatus.ErrorMessage = ex.Message;
            }
            return new { responseStatus, GetList };
        }
        #endregion
    }

}
