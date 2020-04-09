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
        using Microsoft.Dynamics.Retail.Diagnostics;

        public class UnclosedShiftDataServiceAsync
        {
            public static void CreateShiftAsync(CreateShiftRequest request, CreateShiftResponse response)
            {
                ThrowIf.Null(request, "CreateShiftRequest");
                ThrowIf.Null(response, "CreateShiftResponse");

                string inventLocationDataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                Task.Run<bool>(()=> {
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

                Task.Run(() =>
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
                }).Wait();
            }

            public static void CloseShiftAsync(Shift shift, Request request)
            {
                Task.Run<bool>(() =>
                {
                    InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                       "RemoveOpenedShift",
                       shift.StoreRecordId,
                       shift.TerminalId,
                       shift.ShiftId);
                    InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                    ReadOnlyCollection<object> results = RTSResponse.Result;
                    bool success = Convert.ToBoolean(results[0]);
                    return success;
                });

                Task.Run(() =>
                {
                    using (var databaseContext = new DatabaseContext(request))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@CHANNEL"] = shift.StoreRecordId;
                        parameters["@TERMINALID"] = shift.TerminalId;
                        parameters["@SHIFTID"] = shift.ShiftId;

                        databaseContext.ExecuteStoredProcedureNonQuery("[ext].[DeleteClosedShift]", parameters);
                    }
                }).Wait();
            }


            public static void ChangeShiftStatusAsync(Shift shift, Request request)
            {
                Task.Run<bool>(() => {
                    InvokeExtensionMethodRealtimeRequest extensionRequest = new InvokeExtensionMethodRealtimeRequest(
                       "ChangeShiftStatus",
                       shift.StoreRecordId,
                       shift.TerminalId,
                       shift.ShiftId,
                       Convert.ToInt32(shift.Status),
                       shift.CurrentTerminalId,
                       shift.CurrentStaffId);

                    InvokeExtensionMethodRealtimeResponse RTSResponse = request.RequestContext.Execute<InvokeExtensionMethodRealtimeResponse>(extensionRequest);
                    ReadOnlyCollection<object> results = RTSResponse.Result;
                    bool success = Convert.ToBoolean(results[0]);
                    return success;
                });

                Task.Run(() =>
                {
                    using (var databaseContext = new DatabaseContext(request))
                    {
                        ParameterSet parameters = new ParameterSet();
                        parameters["@CHANNEL"] = shift.StoreRecordId;
                        parameters["@TERMINALID"] = shift.TerminalId;
                        parameters["@SHIFTID"] = shift.ShiftId;
                        parameters["@STATUS"] = shift.Status;
                        parameters["@STATUSDATETIMEUTC"] = shift.StatusDateTime;
                        parameters["@CURRENTSTAFFID"] = shift.CurrentStaffId;
                        parameters["@CURRENTTERMINALID"] = shift.CurrentTerminalId;
                        databaseContext.ExecuteStoredProcedureNonQuery("[ext].[ChangeShiftStaus]", parameters);
                    }
                }).Wait();
            }
        }
    }
}

