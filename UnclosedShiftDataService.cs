namespace Contoso
{
    namespace Commerce.Runtime.GetOpenShifts
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using System.Collections.ObjectModel;
        using System.Threading.Tasks;

        public class UnclosedShiftDataService
        {
            public static async Task<bool> CreateShiftRTSAsync(CreateShiftRequest request, CreateShiftResponse response)
            {
                Func<bool> function = new Func<bool>(() =>
                {
                    string inventLocationDataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                    InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                        "CreateNewShift",
                        response.Shift.StoreRecordId,
                        response.Shift.TerminalId,
                        response.Shift.StoreId,
                        response.Shift.ShiftId,
                        response.Shift.StaffId,
                        response.Shift.CurrentStaffId,
                        Convert.ToInt32(response.Shift.Status),
                        response.Shift.CurrentTerminalId,
                        response.Shift.IsShared,
                        response.Shift.CashDrawer,
                        inventLocationDataAreaId);

                    InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                    ReadOnlyCollection<object> results = RTSResponse.Result;
                    bool success = Convert.ToBoolean(results[0]);
                    return success;
                });

               return await Task.Run<bool>(function);
            }

            public static async void CreateShiftV2(CreateShiftRequest request, CreateShiftResponse response)
            {
                ThrowIf.Null(request, "CreateShiftRequest");
                ThrowIf.Null(response, "CreateShiftResponse");

                string inventLocationDataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

               CreateShiftRTSAsync(request, response);

                using (var databaseContext = new DatabaseContext(request))
                {
                    ParameterSet parameters = new ParameterSet();

                    parameters["@CHANNEL"] = response.Shift.StoreRecordId;
                    parameters["@TERMINALID"] = response.Shift.TerminalId;
                    parameters["@STOREID"] = response.Shift.StoreId;
                    parameters["@SHIFTID"] = response.Shift.ShiftId;
                    parameters["@STAFFID"] = response.Shift.StaffId;
                    parameters["@CURRENTSTAFFID"] = response.Shift.CurrentStaffId;
                    parameters["@STATUS"] = response.Shift.Status;
                    parameters["@CURRENTTERMINALID"] = response.Shift.CurrentTerminalId;
                    parameters["@ISSHARED"] = response.Shift.IsShared;
                    parameters["@STARTDATETIMEUTC"] = DateTimeOffsetDataHelper.GetDbNullableDateTime(response.Shift.StartDateTime);
                    parameters["@STATUSDATETIMEUTC"] = DateTimeOffsetDataHelper.GetDbNullableDateTime(response.Shift.StatusDateTime);
                    parameters["@DATAAREAID"] = inventLocationDataAreaId;
                    parameters["@CASHDRAWER"] = response.Shift.CashDrawer;

                    databaseContext.ExecuteStoredProcedureNonQuery("[ext].[CreateNewShift]", parameters);
                }
            }

            public static void CreateShift(CreateShiftRequest request, CreateShiftResponse response)
            {
                ThrowIf.Null(request, "CreateShiftRequest");
                ThrowIf.Null(response, "CreateShiftResponse");

                Shift shift = response.Shift;
                string inventLocationDataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                    "CreateNewShift",
                    response.Shift.StoreRecordId,
                    response.Shift.TerminalId,
                    response.Shift.StoreId,
                    response.Shift.ShiftId,
                    response.Shift.StaffId,
                    response.Shift.CurrentStaffId,
                    Convert.ToInt32(response.Shift.Status),
                    response.Shift.CurrentTerminalId,
                    response.Shift.IsShared,
                    response.Shift.CashDrawer,
                    inventLocationDataAreaId);

                InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                ReadOnlyCollection<object> results = RTSResponse.Result;

                bool success = Convert.ToBoolean(results[0]);

                if (!success)
                {
                    using (var databaseContext = new DatabaseContext(request))
                    {
                        ParameterSet parameters = new ParameterSet();

                        parameters["@CHANNEL"] = response.Shift.StoreRecordId;
                        parameters["@TERMINALID"] = response.Shift.TerminalId;
                        parameters["@STOREID"] = response.Shift.StoreId;
                        parameters["@SHIFTID"] = response.Shift.ShiftId;
                        parameters["@STAFFID"] = response.Shift.StaffId;
                        parameters["@CURRENTSTAFFID"] = response.Shift.CurrentStaffId;
                        parameters["@STATUS"] = response.Shift.Status;
                        parameters["@CURRENTTERMINALID"] = response.Shift.CurrentTerminalId;
                        parameters["@ISSHARED"] = response.Shift.IsShared;
                        parameters["@STARTDATETIMEUTC"] = DateTimeOffsetDataHelper.GetDbNullableDateTime(response.Shift.StartDateTime);
                        parameters["@STATUSDATETIMEUTC"] = DateTimeOffsetDataHelper.GetDbNullableDateTime(response.Shift.StatusDateTime);
                        parameters["@DATAAREAID"] = inventLocationDataAreaId;
                        parameters["@CASHDRAWER"] = response.Shift.CashDrawer;

                        databaseContext.ExecuteStoredProcedureNonQuery("[ext].[CreateNewShift]", parameters);
                    }
                }
            }

            public static void CloseShift(ChangeShiftStatusRequest request, ChangeShiftStatusResponse response)
            {
                ThrowIf.Null(request, "CreateShiftRequest");
                ThrowIf.Null(response, "CreateShiftResponse");

                InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                    "RemoveOpenedShift",
                    response.Shift.StoreRecordId,
                    response.Shift.TerminalId,
                    response.Shift.ShiftId);

                InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                ReadOnlyCollection<object> results = RTSResponse.Result;

                bool success = Convert.ToBoolean(results[0]);

                if (!success)
                {
                    using (var databaseContext = new DatabaseContext(request))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@CHANNEL"] = response.Shift.StoreRecordId;
                        parameters["@TERMINALID"] = response.Shift.TerminalId;
                        parameters["@SHIFTID"] = response.Shift.ShiftId;

                        databaseContext.ExecuteStoredProcedureNonQuery("[ext].[DeleteClosedShift]", parameters);
                    }
                }
            }

            public static void ChangeShiftStatus(ChangeShiftStatusRequest request, ChangeShiftStatusResponse response)
            {
                ThrowIf.Null(request, "CreateShiftRequest");
                ThrowIf.Null(response, "CreateShiftResponse");

                InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                   "ChangeShiftStatus",
                   response.Shift.StoreRecordId,
                   response.Shift.TerminalId,
                   response.Shift.ShiftId,
                   Convert.ToInt32(response.Shift.Status),
                   response.Shift.CurrentTerminalId,
                   response.Shift.CurrentStaffId);

                InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                ReadOnlyCollection<object> results = RTSResponse.Result;

                bool success = Convert.ToBoolean(results[0]);

                if (!success)
                {
                    using (var databaseContext = new DatabaseContext(request))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@CHANNEL"] = response.Shift.StoreRecordId;
                        parameters["@TERMINALID"] = response.Shift.TerminalId;
                        parameters["@SHIFTID"] = response.Shift.ShiftId;
                        parameters["@STATUS"] = response.Shift.Status;
                        parameters["@STATUSDATETIMEUTC"] = response.Shift.StatusDateTime;
                        parameters["@CURRENTSTAFFID"] = response.Shift.CurrentStaffId;
                        parameters["@CURRENTTERMINALID"] = response.Shift.CurrentTerminalId;

                        databaseContext.ExecuteStoredProcedureNonQuery("[ext].[ChangeShiftStaus]", parameters);
                    }
                }
            }
        }
    }
}
