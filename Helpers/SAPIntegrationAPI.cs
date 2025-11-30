using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System;

namespace GBSWarehouse.Helpers
{
    public static class SAPIntegrationAPI
    {
        #region SAP Integration APIs
        public static List<SapPurchaseRequest_Response> GetSapPurchaseRequest(string FRGDT, string RESWK, string MATNR, string WERKS)
        {
            //Request
            //FRGDT	  Types	DATS	8	0	Purchase Requisition Release Date	Mandatory
            //RESWK   Types CHAR    4   0   Supplying(issuing) plant in case of stock transport order  Mandatory
            //MATNR   Types CHAR    18  0   Material Number Optional
            //WERKS   Types CHAR    4   0   Plant Optional


            //Request ex
            //{
            //    "FRGDT": "20230220",
            //    "RESWK": "JBFP",
            //    "MATNR": "",
            //    "WERKS": ""
            //}

            //Response
            //WERKS         Types	CHAR	4	0	Plant
            //BANFN         Types   CHAR    10  0   Purchase requisition number
            //MATNR         Types   CHAR    18  0   Material Number
            //FRGDT         Types   DATS    8   0   Purchase Requisition Release Date
            //BNFPO         Types   NUMC    5   0   Item number of purchase requisition
            //MEINS         Types   UNIT    3   0   Purchase requisition unit of measure
            //LGORT         Types   CHAR    4   0   Storage location
            //messageCode  Types   NUMC    3   0   Message number
            //messageText  Types   CHAR    10  0   PRs Text(Created Or Failed)
            //message       Types   CHAR    200 0   Message Text
            //messageType  Types   CHAR    3   0   Message Type
            //MENGE         Types   QUAN    13  3   Purchase requisition quantity


            //Response Example
            //[
            //    {
            //        "WERKS": "T001",
            //        "BANFN": "0011633866",
            //        "MATNR": "000000000000030052",
            //        "FRGDT": "2023-02-16",
            //        "BNFPO": 10,
            //        "MEINS": "KAR",
            //        "LGORT": "",
            //        "messageCode": 200,
            //        "messageText": "Created",
            //        "message": "",
            //        "messageType": "",
            //        "MENGE": 100.000
            //    },
            //    {
            //        "WERKS": "T001",
            //        "BANFN": "0011633867",
            //        "MATNR": "000000000000030056",
            //        "FRGDT": "2023-02-16",
            //        "BNFPO": 10,
            //        "MEINS": "KAR",
            //        "LGORT": "",
            //        "messageCode": 200,
            //        "messageText": "Created",
            //        "message": "",
            //        "messageType": "",
            //        "MENGE": 150.000
            //    }
            //]

            using (WebClient webClient = new WebClient())
            {
                string UriAddress = string.Empty;
                try
                {
                    GetSapPurchaseRequest_Request obj = new()
                    {
                        FRGDT = FRGDT,
                        RESWK = RESWK,
                        MATNR = MATNR,
                        WERKS = WERKS
                    };

                    UriAddress = string.Format("http://hanaqas.juhaynafood.ind:8000/sap/zgbs_get_prs?sap-client=300");
                    webClient.BaseAddress = UriAddress;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    //webClient.Headers[HttpRequestHeader.Authorization] = "Basic V00uR0JTOldNLjIwMjM=";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    string SerialObj = JsonConvert.SerializeObject(obj);
                    var response = webClient.UploadString(UriAddress, SerialObj);
                    var responseMessage = JsonConvert.DeserializeObject<List<SapPurchaseRequest_Response>>(response);
                    return responseMessage;
                }
                catch (Exception)
                {
                    return new List<SapPurchaseRequest_Response>();
                }
            }
        }
        public static List<SapShipmentRequest_Response> GetSapShipment(List<string> SHTYP, string SHIPMENT_DATE, string PLANT)
        {
            //Request
            //SHTYP	        Types	CHAR	4	0	Shipment type
            //SHIPMENT_DATE Types   DATS    8   0   Planned date of check-in
            //PLANT         Types   CHAR    8   0   plant



            //Request ex
            //{
            //    "SHIPMENTS_TYPES": [
            //                {
            //                "SHTYP": "TR"
            //                },
            //                {
            //                "SHTYP": "ZJTR"
            //                },
            //                {
            //                "SHTYP": "MR"
            //                }
            //                       ],
            //    "SHIPMENT_DATE": "20221208",
            //    "PLANT": "JBFP"
            //}

            //Response
            //TKNUM	Types	CHAR	10	0	Shipment Number
            //TPBEZ Types   CHAR    20  0   Description of Shipment
            //SHTYP   Types CHAR    4   0   Shipment type
            //ROUTE Types   CHAR    6   0   Shipment route
            //routeName Types   CHAR    40  0   Text, 40 Characters Long
            //DPREG Types   DATS    8   0   Planned date of check-in
            //SDABW Types   CHAR    4   0   Special processing indicator
            //indicatorName  Types CHAR    40  0   Text, 40 Characters Long
            //TDLNR Types   CHAR    10  0   Number of forwarding agent
            //NAME1 Types   CHAR    35  0   Name 1


            //Response Example
            //[
            //    {
            //        "tknum": "0003987964",
            //        "tpbez": "jbfp",
            //        "shtyp": "FR",
            //        "route": "F-STOR",
            //        "routeName": "",
            //        "dpreg": "2022-12-08",
            //        "sdabw": "D24",
            //        "indicatorName": "",
            //        "tdlnr": "0003080221",
            //        "name1": ""
            //    },
            //    {
            //        "tknum": "0003987965",
            //        "tpbez": "jbfp",
            //        "shtyp": "FR",
            //        "route": "F-STOR",
            //        "routeName": "",
            //        "dpreg": "2022-12-08",
            //        "sdabw": "D24",
            //        "indicatorName": "",
            //        "tdlnr": "0003080221",
            //        "name1": ""
            //    }
            //]

            using (WebClient webClient = new WebClient())
            {
                string UriAddress = string.Empty;
                try
                {
                    List<SHTYP_Value> SHIPMENTS_TYPES = new List<SHTYP_Value>();
                    foreach (var item in SHTYP)
                    {
                        SHIPMENTS_TYPES.Add(new SHTYP_Value() { SHTYP = item });
                    }

                    GetSapShipmentRequest_Request obj = new()
                    {
                        SHIPMENTS_TYPES = SHIPMENTS_TYPES,
                        SHIPMENT_DATE = SHIPMENT_DATE,
                        PLANT = PLANT
                    };

                    UriAddress = string.Format("http://hanaqas.juhaynafood.ind:8000/sap/zgbs_getshpmnt?sap-client=300");
                    webClient.BaseAddress = UriAddress;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    //webClient.Headers[HttpRequestHeader.Authorization] = "Basic V00uR0JTOldNLjIwMjM=";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    string SerialObj = JsonConvert.SerializeObject(obj);
                    var response = webClient.UploadString(UriAddress, SerialObj);
                    var responseMessage = JsonConvert.DeserializeObject<List<SapShipmentRequest_Response>>(response);
                    return responseMessage;
                }
                catch (Exception)
                {
                    return new List<SapShipmentRequest_Response>();
                }
            }
        }
        //public static CreateSapOrderResponse CreateSapOrder(string PLNBEZ, string WERKS, string PWERK, string AUART, long GAMNG, string GSTRP, string GLTRP, string GMEIN, string VERID)
        //{
        //    //Request
        //    //PLNBEZ  Types   CHAR    18  0   Material Number                 Mandatory
        //    //WERKS   Types   CHAR    4   0   Plant                           Mandatory
        //    //PWERK   Types   CHAR    4   0   Planning plant for the order    Mandatory
        //    //AUART   Types   CHAR    4   0   Order Type                      Mandatory
        //    //GAMNG   Types   QUAN    13  3   Total order quantity            Mandatory
        //    //GSTRP   Types   DATS    8   0   Basic Start Date                Mandatory
        //    //GLTRP   Types   DATS    8   0   Basic finish date               Mandatory
        //    //GMEIN   Types   UNIT    3   0   Base Unit of Measure            Mandatory
        //    //VERID   Types   CHAR    4   0   Production Version              Optional

        //    //Request ex
        //    //{
        //    //    "PLNBEZ": "000000000000010082",
        //    //    "WERKS": "JBFP",
        //    //    "PWERK": "JBFP",
        //    //    "AUART": "PI03",
        //    //    "GAMNG": 1000,
        //    //    "GSTRP": "18.05.2023",// Basic start date in format dd.mm.yyyy
        //    //    "GLTRP": "18.05.2024",// Basic finish date in format dd.mm.yyyy
        //    //    "GMEIN": "CAR",
        //    //    "VERID": "1"
        //    //}

        //    //Response
        //    //PLNBEZ        Types   CHAR    18  0   Material Number 
        //    //WERKS         Types   CHAR    4  0   Plant
        //    //PWERK         Types   CHAR    4  0   Planning plant for the order
        //    //AUART         Types   CHAR    4  0   Order Type
        //    //GAMNG         Types   QUAN    13 3   Total order quantity
        //    //GSTRP         Types   DATS    8  0  Basic Start Date
        //    //GLTRP         Types   DATS    8  0  Basic finish date
        //    //GMEIN         Types   UNIT    3  0  Base Unit of Measure
        //    //VERID         Types   CHAR    4  0  Production Version
        //    //AUFNR        Types   CHAR    12  0   Order Number
        //    //messageCode Types   NUMC    3   0   Message number
        //    //messageText Types   CHAR    10  0   Order Number Text(Created Or Failed)
        //    //message      Types   CHAR    200 0   Message Text
        //    //messageCode2 Types   NUMC    3   0   Message number
        //    //messageText2 Types   CHAR    10  0   Order Number Text(Created Or Failed)
        //    //message2      Types   CHAR    200 0   Message Text

        //    //Response Example
        //    //{
        //    //    "plnbez": "000000000000010082",
        //    //    "werks": "JBFP",
        //    //    "pwerk": "JBFP",
        //    //    "auart": "PI03",
        //    //    "gamng": 1000.000,
        //    //    "gstrp": "18.05.2023",
        //    //    "gltrp": "18.05.2024",
        //    //    "gmein": "KAR",
        //    //    "verid": "1",
        //    //    "aufnr": "000003092296",
        //    //    "messageCode": 0,
        //    //    "messageText": "Created",
        //    //    "message": "",
        //    //    "messageCode2": 0,
        //    //    "messageText2": "Released Successfuly",
        //    //    "message2": ""
        //    //}

        //    using (WebClient webClient = new WebClient())
        //    {
        //        string UriAddress = string.Empty;
        //        try
        //        {
        //            CreateSapOrderParameters obj = new()
        //            {
        //                PLNBEZ = PLNBEZ,
        //                WERKS = WERKS,
        //                PWERK = PWERK,
        //                AUART = AUART,
        //                GAMNG = GAMNG,
        //                GSTRP = GSTRP,
        //                GLTRP = GLTRP,
        //                GMEIN = GMEIN,
        //                VERID = VERID
        //            };

        //            UriAddress = string.Format("http://vhjfiqasci.sap.juhayna.com:8000/sap/zgbs_create_ord");
        //            webClient.BaseAddress = UriAddress;
        //            webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
        //            //webClient.Headers[HttpRequestHeader.Authorization] = "Basic V00uR0JTOldNLjIwMjM=";
        //            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
        //            webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
        //            string SerialObj = JsonConvert.SerializeObject(obj);
        //            var response = webClient.UploadString(UriAddress, SerialObj);
        //            var responseMessage = JsonConvert.DeserializeObject<CreateSapOrderResponse>(response);
        //            return responseMessage;
        //        }
        //        catch (Exception ex)
        //        {
        //            return new CreateSapOrderResponse() { messageText = "Exception Error", message = ex.Message };
        //        }
        //    }
        //}
        public static CreateSapOrderResponse CreateSapOrder(
    string PLNBEZ, string WERKS, string PWERK, string AUART,
    long GAMNG, string GSTRP, string GLTRP, string GMEIN, string VERID)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    CreateSapOrderParameters obj = new()
                    {
                        PLNBEZ = PLNBEZ,
                        WERKS = WERKS,
                        PWERK = PWERK,
                        AUART = AUART,
                        GAMNG = GAMNG,
                        GSTRP = GSTRP,
                        GLTRP = GLTRP,
                        GMEIN = GMEIN,
                        VERID = VERID
                    };

                    string UriAddress = "http://vhjfiqasci.sap.juhayna.com:8000/sap/zgbs_create_ord?sap-client=300";

                    webClient.BaseAddress = UriAddress;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json;";
                    webClient.Encoding = Encoding.UTF8;

                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
                    webClient.Headers[HttpRequestHeader.Authorization] = $"Basic {credentials}";

                    string SerialObj = JsonConvert.SerializeObject(obj);

                    var response = webClient.UploadString(UriAddress, "POST", SerialObj);

                    var responseMessage = JsonConvert.DeserializeObject<CreateSapOrderResponse>(response);

                    return responseMessage ?? new CreateSapOrderResponse()
                    {
                        messageText = "Empty Response",
                        message = "No data returned from SAP"
                    };
                }
                catch (Exception ex)
                {
                    return new CreateSapOrderResponse()
                    {
                        messageText = "Exception Error",
                        message = ex.Message
                    };
                }
            }
        }

        public static CloseBatchResponse CloseBatchAPI(string MATNR, string AUFNR, string MENGE, string CHARG, string HSDAT, string MEINS, string BUDAT, string WERKS, string LGORT)
        {
            //Request
            //MATNR   Types   CHAR    18  0   Material Number                 Mandatory
            //AUFNR   Types   CHAR    12  0   Order Number                    Mandatory
            //MENGE   Types   QUAN    13  3   Quantity                        Mandatory
            //CHARG   Types   CHAR    10  0   Batch Number                    Mandatory
            //HSDAT   Types   DATS    8   0   Date of Manufacture             Mandatory
            //MEINS   Types   UNIT    3   0   Base Unit of Measure            Optional
            //BUDAT   Types   DATS    8   0   Posting Date in the Document    Mandatory
            //WERKS   Types   CHAR    4   0   Plant                           Mandatory
            //LGORT   Types   CHAR    4   0   Storage location                Mandatory

            //Request ex
            //{
            //    "MATNR": "000000000000010082",
            //    "AUFNR": "000003092244",
            //    "MENGE": 100,
            //    "CHARG": "5679854321",
            //    "HSDAT": "2023-01-25",
            //    "MEINS": "",
            //    "BUDAT": "2023-01-25",
            //    "WERKS": "JBFP",
            //    "LGORT": "FG01"
            //}

            //Response
            //MATNR   Types   CHAR    18  0   Material Number
            //AUFNR Types   CHAR    12  0   Order Number
            //MENGE   Types   QUAN    13  3   Quantity   
            //CHARG   Types   CHAR    10  0   Batch Number 
            //HSDAT   Types   DATS    8   0   Date of Manufacture 
            //MEINS   Types   UNIT    3   0   Base Unit of Measure 
            //BUDAT   Types   DATS    8   0   Posting Date in the Document
            //WERKS   Types   CHAR    4   0   Plant
            //LGORT   Types   CHAR    4   0   Storage location    
            //MBLNR	       Types	CHAR	10	0	Number of Material Document
            //MJAHR        Types    NUMC    4   0   Material Document Year
            //messageCode Types    NUMC    3   0   Message number
            //messageText Types    CHAR    10  0   Order Number Text(Created Or Failed)
            //message      Types    CHAR    200 0   Message Text
            //messageType Types    CHAR    3   0   Message Type


            //Response Example
            //{
            //    "matnr": "000000000000010082",
            //    "aufnr": "",
            //    "menge": 0,
            //    "charg": "",
            //    "hsdat": "0000-00-00",
            //    "meins": "",
            //    "budat": "0000-00-00",
            //    "werks": "",
            //    "lgort": "",
            //    "mblnr": "",
            //    "mjahr": 0,
            //    "messageCode": 0,
            //    "messageText": "Plant does",
            //    "message": "",
            //    "messageType": "E"
            //}


            using (WebClient webClient = new WebClient())
            {
                string UriAddress = string.Empty;
                try
                {
                    CloseBatchParameters obj = new()
                    {
                        MATNR = MATNR,
                        AUFNR = AUFNR,
                        MENGE = MENGE,
                        CHARG = CHARG,
                        HSDAT = HSDAT,
                        MEINS = MEINS,
                        BUDAT = BUDAT,
                        WERKS = WERKS,
                        LGORT = LGORT
                    };

                    UriAddress = string.Format("http://vhjfiqasci.sap.juhayna.com:8000/sap/zgbs_closebatch");
                    webClient.BaseAddress = UriAddress;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    //webClient.Headers[HttpRequestHeader.Authorization] = "Basic V00uR0JTOldNLjIwMjM=";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    string SerialObj = JsonConvert.SerializeObject(obj);
                    var response = webClient.UploadString(UriAddress, SerialObj);
                    var responseMessage = JsonConvert.DeserializeObject<CloseBatchResponse>(response);
                    return responseMessage;
                }
                catch (Exception ex)
                {
                    return new CloseBatchResponse() { messageText = "Exception Error", message = ex.Message };
                }
            }
        }
        public static List<SapPostShipmentRequest_Response> SapPostShipment(SapPostShipmentRequestList_Request model)
        {
            //Request
            //TKNUM	 Types 	CHAR	10	0	Shipment Number	                    Mandatory
            //BANFN  Types  CHAR    10  0   Purchase requisition number         Mandatory
            //BNFPO  Types  NUMC    5   0   Item number of purchase requisition Mandatory
            //CHARG  Types  CHAR    10  0   Batch Number                        Mandatory
            //MATNR  Types  CHAR    18  0   Material Number                     Mandatory
            //MENGE  Types  QUAN    13  3   Purchase requisition quantity       Mandatory
            //ROUTE  Types  CHAR    6   0   Shipment route                      Mandatory
            //SIGNI  Types  CHAR    20  0   Container ID                        Mandatory
            //EXTI1  Types  CHAR    20  0   External identification 1           Mandatory
            //DPREG  Types  DATS    8   0   Planned date of check-in	        Mandatory

            //Request ex
            //[
            //    {
            //        "TKNUM": "11221122",
            //        "BANFN": "11111114",
            //        "BNFPO": 11,
            //        "CHARG": "11111",
            //        "MATNR": "11111",
            //        "MENGE": 111.000,
            //        "ROUTE": "1111",
            //        "SIGNI": "11111",
            //        "EXTI1": "111111",
            //        "DPREG": "0000-00-00"
            //    },
            //    {
            //        "TKNUM": "1122112333",
            //        "BANFN": "1111111444",
            //        "BNFPO": 11,
            //        "CHARG": "1111133",
            //        "MATNR": "11111",
            //        "MENGE": 111.000,
            //        "ROUTE": "1111",
            //        "SIGNI": "11111",
            //        "EXTI1": "111111",
            //        "DPREG": "22021218"
            //    }
            //]


            //Response
            //TKNUM	Types	CHAR	10	0	Shipment Number
            //BANFN	Types	CHAR	10	0	Purchase requisition number
            //BNFPO	Types	NUMC	5	0	Item number of purchase requisition
            //CHARG	Types	CHAR	10	0	Batch Number 
            //msgCode  Types   CHAR    3   0   Message Type
            //errorText       Types   CHAR    200 0   Message Text


            //Response Example
            //[
            //            {
            //                "tknum": "11221122",
            //                "banfn": "11111114",
            //                "bnfpo": 11,
            //                "charg": "11111",
            //                "msgCode": 400,
            //                "errorText": "Make sure you pass the Shipment Data"
            //            },
            //            {
            //                "tknum": "1122112333",
            //                "banfn": "1111111444",
            //                "bnfpo": 11,
            //                "charg": "1111133",
            //                "msgCode": 300,
            //                "errorText": "records already sent before"
            //            }
            //            ]

            using (WebClient webClient = new WebClient())
            {
                string UriAddress = string.Empty;
                try
                {
                    SapPostShipmentRequestList_Request obj = model;

                    UriAddress = string.Format("http://hanaqas.juhaynafood.ind:8000/sap/zgbs_postshpmnt?sap-client=300");
                    webClient.BaseAddress = UriAddress;
                    webClient.BaseAddress = UriAddress;
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    //webClient.Headers[HttpRequestHeader.Authorization] = "Basic V00uR0JTOldNLjIwMjM=";
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("WM.GBS:WM.2023"));
                    webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    string SerialObj = JsonConvert.SerializeObject(obj);
                    var response = webClient.UploadString(UriAddress, SerialObj);
                    var responseMessage = JsonConvert.DeserializeObject<List<SapPostShipmentRequest_Response>>(response);
                    return responseMessage;
                }
                catch (Exception)
                {
                    return new List<SapPostShipmentRequest_Response>();
                }
            }
        }
        #endregion
    }
}
