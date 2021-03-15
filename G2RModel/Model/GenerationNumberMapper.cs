using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2RModel.Model
{
    public class GenerationNumberMapper
    {
        private static GenerationNumberMapper _instance;

        public static GenerationNumberMapper Instance
        {
            get => _instance ??= new GenerationNumberMapper();
            set => _instance = value;
        }

        private string[] _genNbrs;

        public GenerationNumberMapper Init(int max, string oldest = null)
        {
            _genNbrs = new string[max+1];
            int nxtIx = 1;
            int g = 1;

            
            if (!string.IsNullOrEmpty(oldest))
            {
                // it can be a number or a letter
                // number is easiest
                if (int.TryParse(oldest, out int o) && o > 0)
                {
                    g = o;
                }
                else
                {
                    char oc = oldest.ToUpper()[0];
                    if (oc >= 'A' && oc <= 'Z')
                    {
                        do
                        {
                            _genNbrs[nxtIx++] = oc.ToString();
                        } while (--oc >= 'A');
                    }
                }
            }

            for (int i = nxtIx; i < _genNbrs.Length; i++, g++)
            {
                _genNbrs[i] = g.ToString();
            }

            return this;
        }

        public string GenerationNumberFor(int ix) => ix >= _genNbrs.Length ? "" : _genNbrs[ix];
    }
}
