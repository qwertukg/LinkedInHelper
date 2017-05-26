namespace LinkedInHelper.Models
{
    class Friend
    {
        public int Id { get; set; }

        public string PageId { get; set; }

        public string Name { get; set; }

        public string Position { get; set; }

        public bool IsMyFriend { get; set; }

        public bool ConnectImpossible { get; set; }

        public bool HisVisitorsInvited { get; set; }

        public bool HisFriendsInvited { get; set; }
    }
}
