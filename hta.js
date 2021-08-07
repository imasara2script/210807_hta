hta = (()=>{
	// ブラウザ側で「window.chrome.webview.postMessage('hta')」を実行すると
	// このスクリプトがページに追加され、htaとして必要な各種機能が使用可能になる。
	
	const WPF = function(){
		var arrCallBack = []
		return {
			send:(機能の名前, 引数, callBack)=>{
				// 引数は階層構造のobjでもWPF側で柔軟に処理できるので、特に制限は無い。
				const WPFに渡すobj = {
					機能の名前 : 機能の名前,
					引数       : 引数
				}
				
				const pM = () => window.chrome.webview.postMessage(JSON.stringify(WPFに渡すobj)) // postMessageで送れるのは文字列だけ。
				
				if(!callBack){return pM()}
				
				let i = 0
				while(arrCallBack[i]){ i++ }
				arrCallBack[i] = callBack
				
				WPFに渡すobj.callBack = i.toString()
				pM()
			},
			callBack:(ind, value)=>{
				// JSON.parseするかどうかはcallBack先に任せる。
				const res = arrCallBack[ind](value)
				
				// !resならcallBackを削除する。(res!=falseならcallBackが複数回呼べるという事)…という感じにしたいんだけどcallBackの戻り値がどうしてもundefindになっちゃう。。
				if(!res){ arrCallBack[ind] = 0 }
			},
			sendPromise:function(機能の名前, 引数){
				const this_ = this
				return new Promise((resolve, reject)=>{
					this_.send(機能の名前, 引数, (v)=>resolve(v))
				})
			}
		}
	}()
	
	const 一文字目を大文字にする = (str)=>{
		return str.charAt(0).toUpperCase() + str.slice(1, str.length)
	}
	
	return {
		// WPFから値を返す際「hta.WPF.callBack(1, "value")」のような形で返しているので
		// 「hta.WPF.callBack」が実行できる状態になっていないとWPFからの値を受け取れない。
		WPF,
		
		ウィンドウ:{
			プロパティ:{
				get:async (プロパティ名)=>{ return WPF.sendPromise('get' + 一文字目を大文字にする(プロパティ名)) },
				set:async (プロパティ名, 値)=>{ return WPF.sendPromise(一文字目を大文字にする(プロパティ名), 値) }
			}
		},
		
		shell:{
			cmd:async (path, パラメータ, 同期モード)=>{
				const 機能名 = 'CMD' + (同期モード ? '' : '非') + '同期'
				return WPF.sendPromise(機能名, {path, パラメータ})
			}
		},
		
		ファイルシステム:{
			textRead :async (path)=>{ return WPF.sendPromise('TextRead', path) },
			textWrite:async (path, value)=>{ return WPF.sendPromise('TextWrite', {path, value}) },
			ディレクトリ:{
				カレント:{
					get:async ()=>{ return WPF.sendPromise('getカレントディレクトリ') },
					set:async (path)=>{ return WPF.sendPromise('カレントディレクトリ', path) }
				}
			}
		},
		
		arguments:{
			get:async ()=>{ return WPF.sendPromise('getArgs') }
		}
	}
})()
