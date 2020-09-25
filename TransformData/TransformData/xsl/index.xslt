<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
<xsl:output method="xml" indent="yes"/>

<xsl:template match="index">
         <ul>
             <xsl:attribute name="class">
               <xsl:text>nLettersIndex</xsl:text>
             </xsl:attribute>
           
              <xsl:for-each select="indexitem">
                <xsl:sort select="@text" order="ascending"/>

                  <li>
                    <xsl:attribute name="onclick">
                      <xsl:text>TopicPanel.ShowLetterIndex(this)</xsl:text>
                    </xsl:attribute>

                    <xsl:attribute name="id">
                      <xsl:value-of select="@text"/>
                    </xsl:attribute>
                    <xsl:value-of select="@text"/>

                  </li>

              </xsl:for-each>
        </ul>
  
        <xsl:for-each select="indexitem">
          <xsl:sort select="@text" order="ascending"/>
            <div>
              <xsl:attribute  name="class">
                <xsl:text>hn lTopicIndex</xsl:text>
              </xsl:attribute>

              <xsl:attribute  name="id">
                <xsl:value-of select="@text"/>
                <xsl:text>-index</xsl:text>
              </xsl:attribute>

              <xsl:if test="child::item[1]">
                <xsl:for-each select="child::item">
                  <xsl:sort select="@text" order="ascending"/>
                    <xsl:call-template name="GetItem" />
                </xsl:for-each>
              </xsl:if>
            </div>              
            
        </xsl:for-each>
        

  </xsl:template>

<xsl:template name="GetItem">
  <div>
  
    <b/>
    
    <span>
      <xsl:value-of select="@text"/>
    </span>


  <xsl:if test="child::item[1] or child::link[1]">
    <div>

      <xsl:attribute  name="class">
        <xsl:text>hn</xsl:text>
      </xsl:attribute>

      <xsl:if test="child::link[1]">
        <xsl:for-each select="child::link" >
            <xsl:call-template name="GetLink" />
        </xsl:for-each>
      </xsl:if>

      <xsl:if test="child::item[1]">
        <xsl:for-each select="child::item" >
          <xsl:sort select="@text" order="ascending"/>
            <xsl:call-template name="GetItem" />
        </xsl:for-each>
      </xsl:if>

    </div>
  </xsl:if>

  </div>
</xsl:template>

<xsl:template name="GetLink">
  <!--
  <div>
    <xsl:attribute  name="class">
      <xsl:text>hn</xsl:text>
    </xsl:attribute>-->

      <span>
        <xsl:attribute name="class">
          <xsl:text>{dId:'</xsl:text>
          <xsl:value-of select="@pid"/>
          <xsl:text>', pId:'</xsl:text>
          <xsl:value-of select="@id"/>
          <xsl:text>'}</xsl:text>
        </xsl:attribute>
        <xsl:variable name="pid">
          <xsl:value-of select="@pid"/>
        </xsl:variable>
        <xsl:value-of select="//content/descendant::item[@id=$pid]/@text"/>
      </span>
      
      <!--
      <xsl:attribute name="onclick">
        <xsl:text>c(this,0);</xsl:text>
        <xsl:if test="@target">
          <xsl:text>window.external.OpenDoc('</xsl:text>
          <xsl:value-of select="@target"/>
          <xsl:text>', '</xsl:text>
          <xsl:value-of select="@id"/>
          <xsl:text>');</xsl:text>
        </xsl:if>
        <xsl:text>return true;</xsl:text>

      </xsl:attribute>


      <xsl:variable name="pid">
        <xsl:value-of select="@pid"/>
      </xsl:variable>
      <xsl:value-of select="//root/content/descendant::item[@id=$pid]/@text"/>
      -->
      
   
<!--    </div>-->
  

</xsl:template>
</xsl:stylesheet>
