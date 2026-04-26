namespace DigiClinicApi.Requests
{
    public class CreateAppointmentRequest
    {
        public int TimeSlotId { get; set; }
        public int ServiceId { get; set; }
        public int? ReferralId { get; set; }
    }
}
