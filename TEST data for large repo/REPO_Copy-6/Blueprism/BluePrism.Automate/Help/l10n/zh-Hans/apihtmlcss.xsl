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
		<h1>业务对象定义</h1>	

<div>本文档中包含的信息是 Blue Prism Limited 的专有信息，未经获授权的 Blue Prism 代表的书面同意，不得披露给第三方。</div>

<div>(c) Blue Prism Limited，</div>

<h2>关于本文档…</h2>
<div>业务对象定义从业务和技术角度描述单个业务对象中 Blue Prism 可用的 API 及其参数和行为。每个对象函数的定义描述接口的业务函数、业务函数的参数和用法，以及持续支持接口所需的任何技术说明（包括对相关对象功能的引用）。业务对象定义 API 是一个双重用途的文档，设计为同时满足业务用户和技术系统支持人员的需求（他们需要可用业务函数的相关信息及详细信息）。就其本身而言，业务对象定义 (Business Object Definition, BOD) 是一个工作文档，在开发和实施过程中可能会变化。</div>

<h2>关于 Blue Prism 业务对象</h2>
<div>Blue Prism 环境中的业务对象（即可以拖到流程上的对象，用于捕获和复制部分业务流程）遵循严格的实施指引。对象的定义和行为，无论是设计时在 Process Studio 中所见的，还是测试时或运行时通过控制室实施的，都使用相同的接口定义（称为对象的功能）。Blue Prism 中使用的所有通用和定制业务对象都具有一个公共属性，即“获取功能”。GetCapabilities 函数会返回 XML 格式的字符串，该字符串定义对象的接口、对象的友好名称（出现在 Process Studio 中）以及任何所需的输入和输出。“业务对象定义”对象会捕获与业务对象相关的每个函数的名称、参数、前提条件和结束点，并将它们转换成 Process Studio 中显示的对象定义。</div>

			<h2>1. <xsl:value-of select="resourceunit/@friendlyname"/></h2>
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
				<h4>结束点：</h4>
				<div>
					<xsl:value-of select="endpoint/@narrative"/>
				</div>
				
				<xsl:if test="inputs|outputs">
				<table>
					<tr>
						<th>参数</th>
						<th>方向</th>
						<th>数据类型</th>
						<th>描述</th>
					</tr>
				<xsl:for-each select="inputs/input|outputs/output">
						<tr>
							<td>
								<xsl:value-of select="@name"/>
							</td>
							<xsl:if test="self::input">
							<td>输入</td>
							</xsl:if>
							<xsl:if test="self::output">
							<td>输出</td>
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




