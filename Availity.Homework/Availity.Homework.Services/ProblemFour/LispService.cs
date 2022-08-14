using System;
using System.Collections.Generic;
using System.Text;

namespace Availity.Homework.Services
{
    public class LispService : ILispService
    {
        public LispService() { }

        public bool Valid(string lispCode)
        {
            return ParenthesesClosedAndNestedCorrectly(lispCode);
        }

        #region Private Methods
        private bool ParenthesesClosedAndNestedCorrectly(string lispCode)
        {
            var st = new Stack<char>();

            for (int i = 0; lispCode.Length > i; i++)
            {
                if (lispCode[i] == '(')
                {
                    st.Push(lispCode[i]);
                }
                else if (lispCode[i] == ')')
                {
                    if (st.Count == 0)
                    {
                        return false;
                    }

                    st.Pop();
                }
            }

            return st.Count <= 0;
        }
        #endregion
    }
}
