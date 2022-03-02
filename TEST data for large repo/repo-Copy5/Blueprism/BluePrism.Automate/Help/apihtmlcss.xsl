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
		<h1>Business Object Definition</h1>	

<div>The information contained in this document
is the proprietary information of Blue Prism Limited and should not be
disclosed to a third party without the written consent of an authorised Blue
Prism representative.</div>

<div>(c) Blue Prism Limited, </div>

<h2>About this document…</h2>
<div>The Business Object Definition describes the APIs available to Blue Prism within a single business object, their parameters and their behaviours from both a business and technical perspective.
The definition of each object function describes the business function of the interface, the parameters and usage of the business function and any technical notes required in the on-going support of the interface, including reference to the capabilities of the object.
The Business Object Definition API is a dual-purpose document designed to serve the needs of both business users and technical system support staff who require information relating to the business functions available and their details.
As such, the BOD is a working document and is subject to change during the course of development and implementation.</div>

<h2>About Blue Prism Business Objects</h2>
<div>Business Objects within the Blue Prism environment (i.e. objects which may be drawn onto a process to capture and replicate a part of a business process) adhere to strict guidelines in their implementation.
The definition and behaviour of the object both as seen in Process Studio during design time and as implemented during test or via Control Room at runtime uses the same interface definition, known as an object’s capablilities.
All business objects used within Blue Prism, generic and bespoke, have a common property - Get Capabilities.
The GetCapabilities function returns an XML formatted string which defines the interfaces for that object, their friendly names (as they appear in Process Studio) and any inputs and outputs that are required.
The Business Object Definition object captures the name, parameters, preconditions and endpoints of each function relating to a business object and translates to the object definition seen within Process Studio.</div>

			<h2>1. <xsl:value-of select="resourceunit/@friendlyname" /></h2>
			<div><xsl:value-of select="resourceunit/@narrative" /></div>
			<xsl:for-each select="resourceunit/action">
				<!-- <a><xsl:attribute name="name" ><xsl:value-of select"@name" /></xsl:attribute> -->
				<h3>
					<xsl:number format="1.1.1. " level="multiple" count="resourceunit|action"/><xsl:value-of select="@name" />
				</h3>
				<!-- </a> -->
				<div>
					<xsl:value-of select="@narrative" />
				</div>
				<h4>Preconditions:</h4>
				<xsl:for-each select="preconditions/condition">
					<div>
						<xsl:value-of select="@narrative" />
					</div>
				</xsl:for-each>
				<h4>Endpoint:</h4>
				<div>
					<xsl:value-of select="endpoint/@narrative" />
				</div>
				
				<xsl:if test="inputs|outputs" >
				<table>
					<tr>
						<th>Parameter</th>
						<th>Direction</th>
						<th>Data Type</th>
						<th>Description</th>
					</tr>
				<xsl:for-each select="inputs/input|outputs/output" >
						<tr>
							<td>
								<xsl:value-of select="@name" />
							</td>
							<xsl:if test="self::input">
							<td>in</td>
							</xsl:if>
							<xsl:if test="self::output">
							<td>out</td>
							</xsl:if>
							
							<td>
								<xsl:value-of select="@type" />
							</td>
							<td>
								<xsl:value-of select="@narrative" />
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




