using FMR.Core;

namespace NoteCounter
{
    public class Module : IRenderModule
    {
        public Module()
        {
            
        }
        public string Description => "�ṩ֧���Զ������ݵ�Midi����������.\n�ر���л: ���� @�������� (ԭCJC) ��˼·.";

        public Version APIVersion => new(1, 0, 0, 0);

        public string Name => "Note Counter";
    }
}
