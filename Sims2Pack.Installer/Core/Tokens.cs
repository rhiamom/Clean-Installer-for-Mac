#nullable disable
/***************************************************************************
 *   Copyright (C) 2004-2007 by Karol Rybak                                *
 *   http://phervers.ModTheSims.info                                       *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
 ***************************************************************************/

using System;
using System.Collections;


namespace Sims2Pack_Installer
{
    public class Tokens : IEnumerable
    {
        private string[] elements;

        public Tokens(string source, char[] delimiters)
        {
            // Parse the string into tokens:
            elements = source.Split(delimiters);
        }

        // IEnumerable Interface Implementation:
        //   Declaration of the GetEnumerator() method
        //   required by IEnumerable
        public IEnumerator GetEnumerator()
        {
            return new TokenEnumerator(this);
        }

        // Inner class implements IEnumerator interface:
        private class TokenEnumerator : IEnumerator
        {
            private int position = -1;
            private Tokens t;

            public TokenEnumerator(Tokens t)
            {
                this.t = t;
            }

            // Declare the MoveNext method required by IEnumerator:
            public bool MoveNext()
            {
                if (position < t.elements.Length - 1)
                {
                    position++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Declare the Reset method required by IEnumerator:
            public void Reset()
            {
                position = -1;
            }

            // Declare the Current property required by IEnumerator:
            public object Current
            {
                get
                {
                    return t.elements[position];
                }
            }
        }
    }

}