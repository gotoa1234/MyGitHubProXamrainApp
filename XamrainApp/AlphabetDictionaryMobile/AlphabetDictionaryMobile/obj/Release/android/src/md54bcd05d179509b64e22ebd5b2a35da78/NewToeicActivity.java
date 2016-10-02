package md54bcd05d179509b64e22ebd5b2a35da78;


public class NewToeicActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer,
		android.speech.tts.TextToSpeech.OnInitListener
{
	static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onInit:(I)V:GetOnInit_IHandler:Android.Speech.Tts.TextToSpeech/IOnInitListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("AlphabetDictionaryMobile.Controller.NewToeic.NewToeicActivity, AlphabetDictionaryMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", NewToeicActivity.class, __md_methods);
	}


	public NewToeicActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NewToeicActivity.class)
			mono.android.TypeManager.Activate ("AlphabetDictionaryMobile.Controller.NewToeic.NewToeicActivity, AlphabetDictionaryMobile, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onInit (int p0)
	{
		n_onInit (p0);
	}

	private native void n_onInit (int p0);

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
