<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">
<html xmlns="http://www.w3.org/TR/xhtml1/strict">
	<style type="text/css" >
		body
		{
			font-family: Arial;
			margin-top: 2.54 cm;
			margin-left: 3.17 cm;
			margin-right: 3.17 cm;
		}
		h4
		{
			margin-bottom: 5px;
		}
		table
		{
			margin-top: 1em;
			width: 100%;
			border:1px black solid;
			border-collapse: collapse;
		}
		th
		{
			padding 5px;
			background-color: #003366;
			color: white;
			border-bottom:2px black solid;
			border-right:1px black solid;
		}
		td
		{
			padding: 5px;
			border-top:1px black solid;
			border-right:1px black solid;
		}
	</style>
	<body>
		<h1>その時点でそのキューでは考慮されなくなります。</h1>	

<div>ビジネスオブジェクト定義本文書に含まれる情報はBlue Prism Limitedの専有情報であり、Blue Prismの代表責任者の書面による同意なく第三者に開示されないものとします。</div>

<div>(c) Blue Prism Limited,</div>

<h2>本文書について...</h2>
<div>ビジネスオブジェクト定義には、単一のビジネスオブジェクト内でBlue Prismで利用可能なAPI、そのパラメーターと振る舞いを業務と技術の両方の観点で記述します。
各オブジェクト機能の定義には、インターフェイスの業務機能、業務機能のパラメーターと使用法、オブジェクトの機能への参照を含む、継続的なインターフェイスのサポートに必要な技術的注意事項を記述します。
ビジネスオブジェクト定義APIは、利用可能な業務機能とその詳細に関する情報を必要とする業務ユーザーと技術システムサポートスタッフの両方のニーズを満たすように設計された、2つの目的を兼ねた文書です。
BODはまだ作業文書ですので、開発と実装の過程で変更される可能性があります。</div>

<h2>Blue Prismビジネスオブジェクトについて</h2>
<div>Blue Prism環境内のビジネスオブジェクト（業務プロセスの一部を取得して複製するためにプロセスに取り込まれる可能性があるオブジェクト）は、厳密なガイドラインに準拠し実装されます。
設計時にプロセススタジオに表示されテスト時に実装される、あるいはランタイムにコントロールルームを介して表示されるオブジェクトの定義と振る舞いは、いずれの場合も、オブジェクトの機能として知られる同じインターフェイス定義を使用します。
Blue Prism内で使用されるすべてのビジネスオブジェクト（汎用およびカスタム）には、共通のプロパティ「Get Capabilities」があります。
GetCapabilities関数は、そのオブジェクトのインターフェイス、わかりやすい名前（プロセススタジオに表示される名前と同じ）、必要な入力と出力を定義するXML形式の文字列を返します。
「ビジネスオブジェクト定義」オブジェクトは、ビジネスオブジェクトに関連する各機能の名前、パラメーター、前提条件、エンドポイントを取得し、プロセススタジオ内で表示されるオブジェクト定義に変換します。</div>

			<h2>1 <xsl:value-of select="resourceunit/@friendlyname"/></h2>
			<div><xsl:value-of select="resourceunit/@narrative"/></div>
			<xsl:for-each select="resourceunit/action">
				<!-- <a><xsl:attribute name="name" ><xsl:value-of select"@name" /></xsl:attribute> -->
				<h3>
					<xsl:number format="1.1.1. " level="multiple" count="resourceunit|action"/><xsl:value-of select="@name"/>
				</h3>
				<!-- </a> -->
				<div>
					<xsl:value-of select="@narrative"/>
				</div>
				<h4>前提条件：</h4>
				<xsl:for-each select="preconditions/condition">
					<div>
						<xsl:value-of select="@narrative"/>
					</div>
				</xsl:for-each>
				<h4>エンドポイント：</h4>
				<div>
					<xsl:value-of select="endpoint/@narrative"/>
				</div>
				
				<xsl:if test="inputs|outputs">
				<table>
					<tr>
						<th>パラメーター</th>
						<th>方向</th>
						<th>データ型</th>
						<th>説明</th>
					</tr>
				<xsl:for-each select="inputs/input|outputs/output">
						<tr>
							<td>
								<xsl:value-of select="@name"/>
							</td>
							<xsl:if test="self::input">
							<td>イン</td>
							</xsl:if>
							<xsl:if test="self::output">
							<td>アウト</td>
							</xsl:if>
							
							<td>
								<xsl:value-of select="@type"/>
							</td>
							<td>
								<xsl:value-of select="@narrative"/>
							</td>
						</tr>
						</xsl:for-each>
				</table>
				</xsl:if>
				
			</xsl:for-each>
	</body>

</html>
</xsl:template>

</xsl:stylesheet>




