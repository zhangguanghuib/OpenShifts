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
        public class CreateOrUpdateShiftsTriggers : IRequestTrigger
        {
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] {
                        typeof(CreateShiftRequest),
                        typeof(ChangeShiftStatusRequest),
                        typeof(ResumeShiftRequest)
                    };
                }
            }

            public void OnExecuted(Request request, Response response)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(response, "response");

                if (request is CreateShiftRequest && response is CreateShiftResponse)
                {
                    this.CreateShift(request as CreateShiftRequest, response as CreateShiftResponse);
                }
                else if (request is ChangeShiftStatusRequest && response is ChangeShiftStatusResponse)
                {
                    Shift shift = ((ChangeShiftStatusResponse)response).Shift;
                    this.ChangeShift(shift, request);
                }
                else if (request is ResumeShiftRequest && response is ResumeShiftResponse)
                {
                    Shift shift = ((ResumeShiftResponse)response).Shift;
                    this.ChangeShift(shift, request);
                }
            }

            public void OnExecuting(Request request)
            {
            }

            private void CreateShift(CreateShiftRequest request, CreateShiftResponse response)
            {
                UnclosedShiftDataServiceAsync.CreateShiftAsync(request, response);
            }

            private void ChangeShift(Shift shift, Request request)
            {
                if(shift.Status == ShiftStatus.Closed)
                {
                    UnclosedShiftDataServiceAsync.CloseShiftAsync(shift, request);
                }
                else
                {
                    UnclosedShiftDataServiceAsync.ChangeShiftStatusAsync(shift, request);
                }
            }

        }
    }
}
