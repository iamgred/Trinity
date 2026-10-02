//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;

namespace Trinity.Shared.Constants
{
    public static class CheckinRequestOffsets
    {
        public const int UUIDOffset = 36;
        public const int ResultCountOffset = 2;
        public const int ResultsOffset = 38;
        public const int ResultIDStartOffset = 0;
        public const int ResultIDOffset = 1;
        public const int ResultStatusOffset = 1;
        public const int ResultSizeOffset = 2;
        public const int SizeOffset = 2;
        public const int CountOffset = 2;
        public const int ResultOutputStartOffset = 4;
    }

    public static class SectionLayout 
    {
        public const int RequestStartIndex = 0;
        public const int CountStartIndex = 36;
        public const int ResultsStartIndex = 38;
        public const int ResultStartIndex = 0;
        public const int ResultStatusStartIndex = 1;
        public const int ResultSizeStartIndex = 2;
        public const int ResultOutputStartIndex = 4;
    }

}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//