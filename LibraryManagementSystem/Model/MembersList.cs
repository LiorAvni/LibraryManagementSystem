using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Model
{
    /// <summary>
    /// Collection of Member objects
    /// </summary>
    public class MembersList : List<Member>
    {
        public MembersList() : base()
        {
        }

        public MembersList(IEnumerable<Member> collection) : base(collection)
        {
        }

        /// <summary>
        /// Find a member by member ID
        /// </summary>
        public Member FindByMemberID(int memberID)
        {
            return this.Find(m => m.MemberID == memberID);
        }

        /// <summary>
        /// Find a member by card number
        /// </summary>
        public Member FindByCardNumber(string cardNumber)
        {
            return this.Find(m => m.CardNumber == cardNumber);
        }

        /// <summary>
        /// Get all active members
        /// </summary>
        public MembersList GetActiveMembers()
        {
            return new MembersList(this.FindAll(m => m.Status?.ToLower() == "active"));
        }

        /// <summary>
        /// Get members with outstanding fines
        /// </summary>
        public MembersList GetMembersWithFines()
        {
            return new MembersList(this.FindAll(m => m.FinesOwed > 0));
        }

        /// <summary>
        /// Get members who have reached their borrowing limit
        /// </summary>
        public MembersList GetMembersAtLimit()
        {
            return new MembersList(this.FindAll(m => m.CurrentBooksCount >= m.MaxBooksAllowed));
        }
    }
}
