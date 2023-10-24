mergeInto(LibraryManager.library, {
  JSLIB_SB_GetSpectrumData : function(arrayPtr){
  	if(window.SpeechBlendWEBGL === undefined){
		alert('Must include SpeechBlend_WEBGL_AudioSpectrum.jslib file for this function to work. See the SpeechBlend documentation on building WebGL projects.');
		return;
	}
	var arr = window.SpeechBlendWEBGL.fa; 
    window.SpeechBlendWEBGL.a.getByteFrequencyData(arr);	
	HEAPU8.set(arr, arrayPtr); 
  },
   
  JSLIB_SB_GetTimeDomainData : function(arrayPtr){
	if(window.SpeechBlendWEBGL === undefined){
		alert('Must include SpeechBlend_WEBGL_AudioSpectrum.jslib file for this function to work. See the SpeechBlend documentation on building WebGL projects.');
		return;
	}
	var arr = window.SpeechBlendWEBGL.la; 
	window.SpeechBlendWEBGL.a.getByteTimeDomainData(arr);
	HEAPU8.set(arr, arrayPtr);
  },

  JSLIB_SB_InitializeBytes : function(size){
	if(window.SpeechBlendWEBGL === undefined){
		alert('Must include SpeechBlend_WEBGL_AudioSpectrum.jslib file for this function to work. See the SpeechBlend documentation on building WebGL projects.');
		return;
	}
	window.SpeechBlendWEBGL.a.fftSize = size * 2;
	window.SpeechBlendWEBGL.fa = new Uint8Array(window.SpeechBlendWEBGL.a.frequencyBinCount);
   	window.SpeechBlendWEBGL.la = new Uint8Array(window.SpeechBlendWEBGL.a.fftSize);
  },
});